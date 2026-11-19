using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
namespace SquashClash.Editor
{
    // Look pass (18 nov): the player's surfaces draw with Rubik (SIL OFL 1.1, Runtime/Visual/Fonts/OFL.txt). A font asset
    // generated at runtime from the .ttf repacked its atlas while the screens were drawn and lost glyphs, so the three
    // weights are baked here once into static SDF font assets with every character the game writes (Spanish included).
    // Regenerate after changing the set or the fonts:
    //   Unity -batchmode -projectPath . -executeMethod SquashClash.Editor.RubikFontAssets.Generate -quit
    public static class RubikFontAssets
    {
        public const string Folder="Assets/SquashClash/Runtime/Visual/Fonts",StyleSheet="Assets/SquashClash/Runtime/Resources/Visual/Hud.uss";
        public static readonly string[] Faces={"Rubik-Regular","Rubik-Bold","Rubik-BoldItalic"};
        private const int SamplingPoints=48,Padding=5,Atlas=1024;
        public static string Characters
        {
            get
            {
                var b=new System.Text.StringBuilder();
                for(char c=' ';c<='~';c++)b.Append(c);
                for(char c=' ';c<='ÿ';c++)b.Append(c);
                b.Append("–—‘’“”…•");
                return b.ToString();
            }
        }
        public static void Generate()
        {
            foreach(var face in Faces)
            {
                var font=AssetDatabase.LoadAssetAtPath<Font>(Folder+"/"+face+".ttf");
                if(font==null)throw new FileNotFoundException(face+".ttf");
                string path=Folder+"/"+face+" SDF.asset";
                AssetDatabase.DeleteAsset(path);
                var asset=FontAsset.CreateFontAsset(font,SamplingPoints,Padding,GlyphRenderMode.SDFAA,Atlas,Atlas,AtlasPopulationMode.Dynamic,false);
                asset.name=face+" SDF";
                // No font features: TextCore read Rubik's kerning pairs wrongly (letters pulled over each other in the
                // italic wordmark), and the pair tables tripled the size of the assets.
                if(!asset.TryAddCharacters(Characters,out string missing,false))Debug.LogWarning("[SquashClash] "+face+" lacks: "+missing);
                AssetDatabase.CreateAsset(asset,path);
                var atlas=asset.atlasTextures[0];atlas.name=face+" Atlas";AssetDatabase.AddObjectToAsset(atlas,asset);
                // Nothing reads the atlas back once it is baked: no CPU copy on the phone.
                var texture=new SerializedObject(atlas);texture.FindProperty("m_IsReadable").boolValue=false;texture.ApplyModifiedPropertiesWithoutUndo();
                asset.material.name=face+" Material";AssetDatabase.AddObjectToAsset(asset.material,asset);
                // Static: what is baked is all it draws; nothing is added (or repacked) while the game runs.
                var so=new SerializedObject(asset);so.FindProperty("m_AtlasPopulationMode").intValue=(int)AtlasPopulationMode.Static;so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                Debug.Log("[SquashClash] font asset "+path+" glyphs="+asset.glyphTable.Count+" characters="+asset.characterTable.Count);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            // The style sheet resolves its font urls when it is imported: import it again now that the assets exist.
            AssetDatabase.ImportAsset(StyleSheet,ImportAssetOptions.ForceUpdate);
        }
    }
}
