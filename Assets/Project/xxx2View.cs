﻿/*********************************************************************************
 *Author:         OnClick
 *Version:        1.0
 *UnityVersion:   2021.3.33f1c1
 *Date:           2024-07-05
*********************************************************************************/
using static IFramework.UI.UnityEventHelper;
namespace IFramework
{
	public class xxx2View : IFramework.UI.MVC.UIView 
	{
//FieldsStart
		private UnityEngine.GameObject heihei_0;
		private UnityEngine.GameObject heihei_1;

//FieldsEnd
		protected override void InitComponents()
		{
		//InitComponentsStart
			heihei_0 = transform.Find("heihei_0@sm").gameObject;
			heihei_1 = transform.Find("heihei_1@sm").gameObject;

		//InitComponentsEnd
		}
		protected override void OnLoad()
		{
		}

		protected override void OnShow()
		{
		}

		protected override void OnHide()
		{
		}

		protected override void OnClose()
		{
		}
	}
}
