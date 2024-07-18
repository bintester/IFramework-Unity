/*********************************************************************************
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
		private UnityEngine.CanvasRenderer xxx0;
		private UnityEngine.GameObject heihei;
		private UnityEngine.CanvasRenderer Image;

//FieldsEnd
		protected override void InitComponents()
		{
		//InitComponentsStart
			xxx0 = transform.Find("xxx0").GetComponent<UnityEngine.CanvasRenderer>();
			heihei = transform.Find("heihei@sm").gameObject;
			Image = transform.Find("xxx/Image@sm").GetComponent<UnityEngine.CanvasRenderer>();

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
