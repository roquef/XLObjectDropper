﻿using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLObjectDropper.UI.Controls.ControllerButtons;

namespace XLObjectDropper.UI.Menus
{
	public class OptionsMenuUI : MonoBehaviour
	{
		[Header("Options Menu Elements")]
		public GameObject MainUI;
		[Space(10)]
		public Slider Sensitivity;
		public Toggle InvertCamControl;
		public Toggle InvertCamControlXAxis;
		public Toggle ShowGrid;

		[Space(10)]
		public Button UndoButton;
		public Button RedoButton;
		public Button ClearAllButton;
		public Button SaveButton;
		public Button LoadButton;

		[Space(10)]
		public BottomRowController BottomRow;

		public GameObject LoadSavedUI;
		[HideInInspector] public bool LoadSavedActive => LoadSavedUI != null && LoadSavedUI.activeInHierarchy;
		public GameObject SaveUI;
		[HideInInspector] public bool SaveActive => SaveUI != null && SaveUI.activeInHierarchy;

		public Animator Animator;

		private void OnEnable()
		{
			SetDefaultState(true);
			Animator.Play("SlideIn");
		}

		private void Start()
		{
			SetDefaultState(true);
		}

		private void Update()
		{
			if (UIManager.Instance.Player.GetButtonDown("B"))
			{
				if (LoadSavedActive)
				{
					StartCoroutine(DisableLoadSavedUI());
					EventSystem.current.SetSelectedGameObject(LoadButton.gameObject);
					SetAllInteractable(true);
				}

				if (SaveActive)
				{
					StartCoroutine(DisableSaveUI());
					EventSystem.current.SetSelectedGameObject(SaveButton.gameObject);
					SetAllInteractable(true);
				}
			}
		}

		public IEnumerator DisableLoadSavedUI()
		{
			LoadSavedUI.GetComponent<LoadSavedUI>().Animator.Play("SlideOut");
			yield return new WaitForSeconds(0.2f);
			LoadSavedUI.SetActive(false);
		}

		public IEnumerator DisableSaveUI()
		{
			SaveUI.GetComponent<SaveUI>().Animator.Play("SlideOut");
			yield return new WaitForSeconds(0.2f);
			SaveUI.SetActive(false);
		}

		private void SetDefaultState(bool isEnabled)
		{
			LoadSavedUI.SetActive(false);
			SaveUI.SetActive(false);

			SetDefaultState(Sensitivity, isEnabled);
			SetDefaultState(InvertCamControl, isEnabled);
			SetDefaultState(InvertCamControlXAxis, isEnabled);
			SetDefaultState(ShowGrid, isEnabled);
			SetDefaultState(UndoButton, isEnabled);
			SetDefaultState(RedoButton, isEnabled);
			SetDefaultState(ClearAllButton, isEnabled);
			SetDefaultState(SaveButton, isEnabled);
			SetDefaultState(LoadButton, isEnabled);

			if (isEnabled)
			{
				SaveButton.onClick.AddListener(SaveClicked);
				LoadButton.onClick.AddListener(LoadClicked);
			}
			else
			{
				SaveButton.onClick.RemoveListener(SaveClicked);
				LoadButton.onClick.RemoveListener(LoadClicked);
			}
		}

		private void SaveClicked()
		{
			SaveUI.SetActive(true);
			SetAllInteractable(false);

			EventSystem.current.SetSelectedGameObject(SaveUI.GetComponent<SaveUI>().InputField.gameObject);
		}

		private void LoadClicked()
		{
			LoadSavedUI.SetActive(true);
			SetAllInteractable(false);
		}

		private void SetDefaultState(Slider slider, bool isEnabled)
		{
			slider.interactable = true;
			slider.gameObject.SetActive(isEnabled);
		}

		private void SetDefaultState(Toggle toggle, bool isEnabled)
		{
			toggle.interactable = true;
			toggle.gameObject.SetActive(isEnabled);
		}

		private void SetDefaultState(Button button, bool isEnabled)
		{
			button.interactable = true;
			button.gameObject.SetActive(isEnabled);
		}

		private void SetAllInteractable(bool interactable)
		{
			Sensitivity.interactable = interactable;
			InvertCamControl.interactable = interactable;
			ShowGrid.interactable = interactable;
			UndoButton.interactable = interactable;
			RedoButton.interactable = interactable;
			ClearAllButton.interactable = interactable;
			SaveButton.interactable = interactable;
			LoadButton.interactable = interactable;
		}

		private void OnDisable()
		{
			SetDefaultState(false);
		}
	}
}
