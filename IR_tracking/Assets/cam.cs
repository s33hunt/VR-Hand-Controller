using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cam : MonoBehaviour
{
	[Range(0,255)] public int threshold = 200;
	[Range(100, 10000)] public int maxBlobPasses = 1000;
	public Renderer targetRend;
	Renderer camrend;
	WebCamTexture webcamTexture;
	Texture2D tex,t;

	void Start()
	{
		tex = Instantiate(targetRend.material.mainTexture) as Texture2D;
		targetRend.material.mainTexture = tex;

		webcamTexture = new WebCamTexture(WebCamTexture.devices[1].name);
		camrend = GetComponent<Renderer>();
		camrend.material.mainTexture = webcamTexture;
		webcamTexture.Play();

		t = new Texture2D(webcamTexture.width, webcamTexture.height);

		framerate = 1f / 30f;
		StartCoroutine("UpdateStuff");
	}
	
	class pixel
	{
		public int x, y, index;
		public Color32 color;
		public pixel(int x, int y, int index, Color32 color)
		{
			this.x = x;
			this.y = y;
			this.index = index;
			this.color = color;
		}
	}

	List<pixel> pixels = new List<pixel>(); //valid pixz
	List<int> checkedPixelIndexes = new List<int>();
	Dictionary<int, List<pixel>> blobs = new Dictionary<int, List<pixel>>(); //seporated blob lists
	Dictionary<int, List<pixel>> 
		pixelMapX = new Dictionary<int, List<pixel>>(),
		pixelMapY = new Dictionary<int, List<pixel>>();




	float framerate;
	IEnumerator UpdateStuff()
	{
		while (true)
		{
			yield return new WaitForSeconds(framerate);

			Color32[] 
				camPixels = webcamTexture.GetPixels32(),
				newPix = new Color32[t.width * t.height];

			int y = 0;
			for (int i = 0; i < camPixels.Length; i+=2)
			{

				int x = (i % webcamTexture.width) /2;
				if (x == 0) { y++; }
				if (y % 2 == 0) { continue; }

				

				//if below threshold, make black
				if (camPixels[i].r < threshold && camPixels[i].g < threshold && camPixels[i].b < threshold)
				{
					camPixels[i].g = 0;
					camPixels[i].r = 0;
					camPixels[i].b = 0;
					//else add it to pixel list for processing
				}
				else {
					pixel p = new pixel(x, y/2, i, camPixels[i]);
					pixels.Add(p);
					if (!pixelMapX.ContainsKey(x)) { pixelMapX.Add(x, new List<pixel>()); }
					pixelMapX[x].Add(p);
				}
			}

			foreach (var p in pixels)
			{
				if (checkedPixelIndexes.Contains(p.index)) { continue; }
				CheckAdjacentRecursive(p, blobs.Count);
			}


			for (int i = 0; i < blobs.Count; i++)
			{
				foreach (pixel p in blobs[i])
				{
					float c = (float)blobs.Count / (float)i;
					camPixels[p.index] = new Color(0, c, 1 - c, 1);
				}
			}

			t.SetPixels32(camPixels);
			t.Apply();
			targetRend.material.mainTexture = t;

			Reset();
		}
	}

	int safety = 0;
	void CheckAdjacentRecursive(pixel p, int blobIndex)
	{
		safety++;
		if(safety > maxBlobPasses) { return; }
		//continue if pixel in used list
		if (checkedPixelIndexes.Contains(p.index)) { return; }
		checkedPixelIndexes.Add(p.index);
		//create target blob if not exist
		if (!blobs.ContainsKey(blobIndex)) { blobs.Add(blobIndex, new List<pixel>()); }
		
		List<pixel> checkList = new List<pixel>();

		//check adjacent pixels
		for (int i = -1; i <= 1; i++) {
			int xval = p.x + i;
			if (pixelMapX.ContainsKey(xval)) {
				foreach (pixel ypix in pixelMapX[xval]) {
					//get adjacent
					if (Mathf.Abs(ypix.y - p.y) <= 1) { checkList.Add(ypix); }
				}
			}
		}
		
		//for those, check and get adjacent
		foreach(pixel c in checkList)
		{
			blobs[blobIndex].Add(c);
			CheckAdjacentRecursive(c, blobIndex);
		}
		//when non left, create blob
		//continue loop
	}


	void Reset()
	{
		safety = 0;
		pixels.Clear();
		checkedPixelIndexes.Clear();
		blobs.Clear();
		pixelMapX.Clear();
		pixelMapY.Clear();
	}
}








/*
using UnityEngine;
using System.Collections;

public class cam : MonoBehaviour
{
	[Range(0,255)]public int threshold = 200;
	public Renderer targetRend;
	Renderer camrend;
	WebCamTexture webcamTexture;
	Texture2D tex,t;

	void Start()
	{
		tex = Instantiate(targetRend.material.mainTexture) as Texture2D;
		targetRend.material.mainTexture = tex;

		webcamTexture = new WebCamTexture(WebCamTexture.devices[1].name);
		camrend = GetComponent<Renderer>();
		camrend.material.mainTexture = webcamTexture;
		webcamTexture.Play();

		t = new Texture2D(webcamTexture.width, webcamTexture.height);
	}
	
	void Update()
	{
		Color32[] camPixels = webcamTexture.GetPixels32();

		for (int i = 0; i < camPixels.Length; i++)
		{
			
			if (camPixels[i].r < threshold && camPixels[i].g < threshold && camPixels[i].b < threshold) {
				camPixels[i].g = 0;
				camPixels[i].r = 0;
				camPixels[i].b = 0;
			}
			
		}

		t.SetPixels32(camPixels);
		t.Apply();
		targetRend.material.mainTexture = t;		
	}
}

*/
