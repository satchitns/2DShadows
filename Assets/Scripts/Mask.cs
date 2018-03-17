using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask : MonoBehaviour
{
	Texture2D tex;
	int texWidth, texHeight;
	float maskThreshold = 2;
	// Use this for initialization
	void Start()
	{
		texWidth = texHeight = 512;
		GenerateTexture();
	}

	// Update is called once per frame
	void Update()
	{

	}

	void GenerateTexture()
	{

		tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, true);
		Vector2 maskCenter = new Vector2(texWidth * 0.5f, texHeight * 0.5f);

		for (int y = 0; y < tex.height; ++y)
		{
			for (int x = 0; x < tex.width; ++x)
			{

				float distFromCenter = Vector2.Distance(maskCenter, new Vector2(x, y));
				float maskPixel = (0.5f - (distFromCenter / texWidth)) * maskThreshold;
				tex.SetPixel(x, y, new Color(maskPixel, maskPixel, maskPixel, 1 - maskPixel));
			}
		}
		tex.Apply();
		GetComponent<MeshRenderer>().material.mainTexture = tex;
	}
}
