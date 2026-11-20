#!/usr/bin/env python3
"""Verify Unity source exports without invoking Xcode or signing."""
import hashlib
import json
from pathlib import Path
import plistlib
import struct
import subprocess
import sys

root = Path(sys.argv[1])
profile = sys.argv[2]
export = root / 'Builds' / ('iOS-' + profile)
with (export / 'Info.plist').open('rb') as stream:
    info = plistlib.load(stream)
for key, expected in [('ITSAppUsesNonExemptEncryption', False),
                      ('UIFileSharingEnabled', True), ('LSSupportsOpeningDocumentsInPlace', True)]:
    if info.get(key) is not expected:
        raise SystemExit('Missing/incorrect plist flag: ' + key)
manifests = [path for path in export.rglob('PrivacyInfo.xcprivacy')]
app_manifests = [path for path in manifests if 'Libraries' not in path.parts]
if not app_manifests:
    raise SystemExit('Missing PrivacyInfo.xcprivacy in the exported Xcode project')
privacy_ok = False
for path in app_manifests:
    with path.open('rb') as stream:
        manifest = plistlib.load(stream)
    declared = {entry['NSPrivacyAccessedAPIType']: entry['NSPrivacyAccessedAPITypeReasons']
                for entry in manifest.get('NSPrivacyAccessedAPITypes', [])}
    if (manifest.get('NSPrivacyTracking') is False and manifest.get('NSPrivacyTrackingDomains') == []
            and manifest.get('NSPrivacyCollectedDataTypes') == []
            and declared.get('NSPrivacyAccessedAPICategoryUserDefaults') == ['CA92.1']
            and declared.get('NSPrivacyAccessedAPICategoryFileTimestamp') == ['C617.1']):
        privacy_ok = True
if not privacy_ok:
    raise SystemExit('Exported PrivacyInfo.xcprivacy does not match the app privacy declaration')
if 'PrivacyInfo.xcprivacy' not in (export / 'Unity-iPhone.xcodeproj/project.pbxproj').read_text():
    raise SystemExit('PrivacyInfo.xcprivacy is not referenced by the Xcode project')
count = subprocess.check_output(['git', 'rev-list', '--count', 'HEAD'], cwd=root, text=True).strip()
if info.get('CFBundleVersion') != count:
    raise SystemExit('CFBundleVersion does not equal the current Git commit count')
for key in ['CFBundleIcons', 'CFBundleIcons~ipad']:
    if info.get(key, {}).get('CFBundlePrimaryIcon', {}).get('CFBundleIconName') != 'AppIcon':
        raise SystemExit('Missing AppIcon plist reference: ' + key)


def png_size(path):
    data = path.read_bytes()
    if data[:8] != b'\x89PNG\r\n\x1a\n':
        raise SystemExit('Not a PNG: ' + str(path))
    return struct.unpack('>II', data[16:24])


catalog = export / 'Unity-iPhone/Images.xcassets/AppIcon.appiconset'
images = json.loads((catalog / 'Contents.json').read_text())['images']
marketing = []
for image in images:
    path = catalog / image.get('filename', '')
    if not path.is_file():
        raise SystemExit('Unbound exported icon: ' + repr(image))
    size = float(image['size'].split('x')[0]) * float(image['scale'].removesuffix('x'))
    if png_size(path) != (int(size), int(size)):
        raise SystemExit('Incorrect exported icon dimensions: ' + str(path))
    if image.get('idiom') == 'ios-marketing' and size == 1024:
        marketing.append(path)
if len(marketing) != 1:
    raise SystemExit('Exactly one exported 1024 px marketing icon is required')
launch_names = {value for key, value in info.items() if key.startswith('UILaunchStoryboardName')}
if not launch_names:
    raise SystemExit('Missing launch storyboard plist reference')
for name in launch_names:
    paths = list(export.rglob(name + '.storyboard'))
    if len(paths) != 1 or 'LaunchScreen' not in paths[0].read_text():
        raise SystemExit('Missing launch storyboard/images: ' + name)
launch = list(export.rglob('LaunchScreen-iPhonePortrait.png'))
if len(launch) != 1 or png_size(launch[0]) != (1290, 2796):
    raise SystemExit('Missing/full-resolution launch artwork is not exported')
release_path = root / 'Logs/evidence' / ('release-' + profile + '.json')
release = json.loads(release_path.read_text())
release['iosRelease'] = {
    'buildNumber': count,
    'plistFlagsVerified': True,
    'privacyManifests': sorted(str(path.relative_to(export)) for path in manifests),
    'iconCount': len(images),
    'marketingIcon1024': str(marketing[0].relative_to(export)),
    'marketingIconSha256': hashlib.sha256(marketing[0].read_bytes()).hexdigest(),
    'launchStoryboards': sorted(launch_names),
    'launchDimensions': list(png_size(launch[0])),
    'launchSha256': hashlib.sha256(launch[0].read_bytes()).hexdigest(),
}
release_path.write_text(json.dumps(release, indent=2) + '\n')
print('IOS-EXPORT-CHECK: ' + profile + ' build=' + count + ' icon=1024 launch=1290x2796 flags=OK')
