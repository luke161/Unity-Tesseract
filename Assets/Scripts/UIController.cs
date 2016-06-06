using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIController : MonoBehaviour {

	private const float kRotationSpeed = 14f;

	public Tesseract target;
	public Camera camera;

	[SerializeField] private Toggle _toggleProjection;
	[SerializeField] private Toggle _toggleAutoRotate;
	[SerializeField] private Slider _sliderXY;
	[SerializeField] private Slider _sliderYZ;
	[SerializeField] private Slider _sliderZX;
	[SerializeField] private Slider _sliderXW;
	[SerializeField] private Slider _sliderYW;
	[SerializeField] private Slider _sliderZW;

	protected void Start()
	{
		_toggleProjection.onValueChanged.AddListener(HandleProjectionChanged);
		_toggleAutoRotate.onValueChanged.AddListener(HandleRotationChanged);

		HandleProjectionChanged(_toggleProjection.isOn);
		HandleRotationChanged(_toggleAutoRotate.isOn);
	}

	protected void Update()
	{
		if(_toggleAutoRotate.isOn){

			float amount = kRotationSpeed*Time.deltaTime;
			target.rotationXY += amount;
			target.rotationZX += amount;
			target.rotationYW += amount;

			UpdateSliders();
		} else {

			target.rotationXY = 360*_sliderXY.value;
			target.rotationYZ = 360*_sliderYZ.value;
			target.rotationZX = 360*_sliderZX.value;
			target.rotationXW = 360*_sliderXW.value;
			target.rotationYW = 360*_sliderYW.value;
			target.rotationZW = 360*_sliderZW.value;
		}
	}

	private void HandleProjectionChanged(bool value)
	{
		camera.orthographic = !value;
		target.useOrthoProjection = !value;
	}

	private void HandleRotationChanged(bool value)
	{
		UpdateSliders();
	}

	private void UpdateSliders()
	{
		UpdateSlider(_sliderXY,target.rotationXY,!_toggleAutoRotate.isOn);
		UpdateSlider(_sliderYZ,target.rotationYZ,!_toggleAutoRotate.isOn);
		UpdateSlider(_sliderZX,target.rotationZX,!_toggleAutoRotate.isOn);
		UpdateSlider(_sliderXW,target.rotationXW,!_toggleAutoRotate.isOn);
		UpdateSlider(_sliderYW,target.rotationYW,!_toggleAutoRotate.isOn);
		UpdateSlider(_sliderZW,target.rotationZW,!_toggleAutoRotate.isOn);
	}

	private void UpdateSlider(Slider slider, float rotation, bool enabled)
	{
		slider.interactable = enabled;
		slider.value = (rotation%360f)/360f;
	}

}
