using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
class ImageEditor : Editor{

	class DeltaColor{
		public float dR;
		public float dG;
		public float dB;
		public float dA;
	};


	static string getResourcePath (string path) {
		string result = path.Replace("Assets/Resources/", "");
		result = result.Replace(".png", "");
		return result;
	}

	[MenuItem ("Image/Format")]
	static void Format() {
		UnityEngine.Object[] SelectedAsset = Selection.GetFiltered (typeof(UnityEngine.Object), SelectionMode.DeepAssets);
		for (int i = 0; i< SelectedAsset.Length; i++) {
			string path = AssetDatabase.GetAssetPath (SelectedAsset [i]);
			FormatTexture (path);
		}
	}

	static void FormatTexture(string path) {
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		if (textureImporter != null) {
			textureImporter.textureType = TextureImporterType.Advanced;
			textureImporter.isReadable = true;
			textureImporter.textureFormat = TextureImporterFormat.ARGB32;
			textureImporter.mipmapEnabled = false;
			textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.generateCubemap = TextureImporterGenerateCubemap.None;
			textureImporter.filterMode = FilterMode.Trilinear;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
			AssetDatabase.ImportAsset(path); 	
		}

	}

	[MenuItem ("Image/Convert")]
	static void Convert()
	{ 
		UnityEngine.Object[] SelectedAsset = Selection.GetFiltered (typeof(UnityEngine.Object), SelectionMode.DeepAssets);
		for (int i = 0; i< SelectedAsset.Length; i++) {
			string path = AssetDatabase.GetAssetPath(SelectedAsset[i]);
			FormatTexture(path);


			string resourcePath = getResourcePath(path);

			Texture2D texture = Resources.Load(resourcePath) as Texture2D;
			int width = texture.width;
			int height = texture.height;
			List <DeltaColor> delta = new List<DeltaColor>();
			for (int index = 0; index < width * height; index++) {
				delta.Add(new DeltaColor());
			}
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					Color scolor = texture.GetPixel(x, y);
					int sr = (int)(scolor.r * 255f);
					int sg = (int)(scolor.g * 255f);
	             	int sb = (int)(scolor.b * 255f);
	                int sa = (int)(scolor.a * 255f);

					sr += (int)delta[y * width + x].dR;
					sg += (int)delta[y * width + x].dG;
					sb += (int)delta[y * width + x].dB;
					sa += (int)delta[y * width + x].dA;

					int colorBit = 16;//16 for RGBA444 ,8 for RGBA555
					int dr = sr % colorBit;
					int dg = sg % colorBit;
					int db = sb % colorBit;
					int da = sa % colorBit;

					if (dr != 0) {
						sr -= dr;
					}
					if (dg != 0) {
						sg -= dg;
					}
					if (db != 0) {
						sb -= db;
					}
					if (da != 0) {
						sa -= da;
					}
					if (x < width - 1) {
						delta[y * width + x + 1].dR += dr * 3f/8f;
						delta[y * width + x + 1].dG += dg * 3f/8f;
						delta[y * width + x + 1].dB += db * 3f/8f;
						delta[y * width + x + 1].dA += da * 3f/8f;
						if (y < height - 1) {
							delta[(y + 1) * width + x + 1].dR += dr * 2f/8f;
							delta[(y + 1) * width + x + 1].dG += dg * 2f/8f;
							delta[(y + 1) * width + x + 1].dB += db * 2f/8f;
							delta[(y + 1) * width + x + 1].dA += da * 2f/8f;
						}
						
					}
					if (y < height - 1) {
						delta[(y + 1) * width + x].dR += dr * 3f/8f;
						delta[(y + 1) * width + x].dG += dg * 3f/8f;
						delta[(y + 1) * width + x].dB += db * 3f/8f;
						delta[(y + 1) * width + x].dA += da * 3f/8f;
					}
					Color newColor = new Color(sr/255f, sg/255f, sb/255f, sa/255f);
					texture.SetPixel(x,  y, newColor);
	
				}
			}
			string outputPath = Application.dataPath + path.Replace("Assets", "");
			outputPath = outputPath.Replace(".png", "@16x.png");
			SetData(outputPath, texture.EncodeToPNG());
			FormatTexture(path.Replace(".png", "@16x.png"));
		}
	}

	public static void SetData(string path, byte[] Datas)
	{
		using(FileStream FS = File.OpenWrite(path))
		{
			Debug.LogError("SetData:" + path);
			FS.Write(Datas, 0, Datas.Length);
			FS.Close();
			FS.Dispose();
		}
	}
}