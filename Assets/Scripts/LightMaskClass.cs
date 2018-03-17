using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMaskClass : MonoBehaviour
{
	private static LightMaskClass LMC = null;
	public LayerMask mask;
	public Material lightMaterial;
	public static LightMaskClass Instance { get { return LMC; } }
	void Awake()
	{
		if (LMC != null)
			GameObject.Destroy(LMC);
		else
			LMC = this;

		DontDestroyOnLoad(this);
	}
}
