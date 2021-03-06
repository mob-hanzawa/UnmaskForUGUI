﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace Coffee.UIExtensions
{
	/// <summary>
	/// Reverse masking for parent Mask component.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("UI/Unmask/Unmask", 1)]
	public class Unmask : MonoBehaviour, IMaterialModifier
	{
		//################################
		// Constant or Static Members.
		//################################
		static readonly Vector2 s_Center = new Vector2(0.5f, 0.5f);


		//################################
		// Serialize Members.
		//################################
		[Tooltip("Fit graphic's transform to target transform on LateUpdate.")]
		[SerializeField] RectTransform m_AutoFitTarget;
		[Tooltip("Show the graphic that is associated with the unmask render area.")]
		[SerializeField] bool m_ShowUnmaskGraphic = false;


		//################################
		// Public Members.
		//################################
		/// <summary>
		/// The graphic associated with the unmask.
		/// </summary>
		public Graphic graphic{ get { return _graphic ?? (_graphic = GetComponent<Graphic>()); } }

		/// <summary>
		/// Fit graphic's transform to target transform on LateUpdate.
		/// </summary>
		public RectTransform autoFitTarget{ get { return m_AutoFitTarget; } set { m_AutoFitTarget = value; } }

		/// <summary>
		/// Show the graphic that is associated with the unmask render area.
		/// </summary>
		public bool showUnmaskGraphic
		{
			get { return m_ShowUnmaskGraphic; }
			set
			{
				m_ShowUnmaskGraphic = value;
				SetDirty();
			}
		}

		/// <summary>
		/// Perform material modification in this function.
		/// </summary>
		/// <returns>Modified material.</returns>
		/// <param name="baseMaterial">Configured Material.</param>
		public Material GetModifiedMaterial(Material baseMaterial)
		{
			if (!isActiveAndEnabled)
			{
				return baseMaterial;
			}

			Transform stopAfter = MaskUtilities.FindRootSortOverrideCanvas(transform);
			var stencilDepth = MaskUtilities.GetStencilDepth(transform, stopAfter);

			StencilMaterial.Remove(_unmaskMaterial);
			_unmaskMaterial = StencilMaterial.Add(baseMaterial, (1 << stencilDepth) - 1, StencilOp.Zero, CompareFunction.Always, m_ShowUnmaskGraphic ? ColorWriteMask.All : (ColorWriteMask)0, 0, (1 << stencilDepth) - 1);
			//StencilMaterial.Remove (baseMaterial);

			return _unmaskMaterial;
		}

		/// <summary>
		/// Fit to target transform.
		/// </summary>
		/// <param name="target">Target transform.</param>
		public void FitTo(RectTransform target)
		{
			var rt = transform as RectTransform;

			rt.position = target.position;
			rt.rotation = target.rotation;

			var s1 = target.lossyScale;
			var s2 = rt.parent.lossyScale;
			rt.localScale = new Vector3(s1.x / s2.x, s1.y / s2.y, s1.z / s2.z);
			rt.sizeDelta = target.rect.size;
			rt.anchorMax = rt.anchorMin = s_Center;
		}


		//################################
		// Private Members.
		//################################
		Material _unmaskMaterial;
		Graphic _graphic;

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		void OnEnable()
		{
			if (m_AutoFitTarget)
			{
				FitTo(m_AutoFitTarget);
			}
			SetDirty();
		}

		/// <summary>
		/// This function is called when the behaviour becomes disabled () or inactive.
		/// </summary>
		void OnDisable()
		{
			StencilMaterial.Remove(_unmaskMaterial);
			_unmaskMaterial = null;
			SetDirty();
		}

		/// <summary>
		/// LateUpdate is called every frame, if the Behaviour is enabled.
		/// </summary>
		void LateUpdate()
		{
			if (m_AutoFitTarget)
			{
				FitTo(m_AutoFitTarget);
			}
		}

		/// <summary>
		/// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
		/// </summary>
		void OnValidate()
		{
			SetDirty();
		}

		/// <summary>
		/// Mark the graphic as dirty.
		/// </summary>
		void SetDirty()
		{
			if (graphic)
			{
				graphic.SetMaterialDirty();
			}
		}
	}
}