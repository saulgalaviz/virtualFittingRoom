using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ModulesUI : MonoBehaviour 
{
  [SerializeField] bool depthOn = true;
  [SerializeField] bool colorOn = true;
  [SerializeField] bool userOn = true;
  [SerializeField] bool skeletonOn = true;
  [SerializeField] bool handsOn = true;
  [SerializeField] bool gesturesOn = true;
  [SerializeField] bool bordersOn = true;

	[SerializeField]GameObject settingsContainer;

    [SerializeField] Toggle
        tDepth,
        tColor,
		tUser,
		tSkeleton,
		tHands,
		tGestures,
    tDepthMesh,
    tBackground,
    tDepthVis;

	NuitrackModules nuitrackModules;

  bool depthVisMesh = false;

  public void ToggleSettings()
  {
    settingsContainer.SetActive(!settingsContainer.activeSelf);
  }

  void Start () 
  {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    settingsContainer.SetActive(false);
    nuitrackModules = FindObjectOfType<NuitrackModules>();

    depthOn = tDepth.isOn;
    colorOn = tColor.isOn;
    userOn = tUser.isOn;
    skeletonOn = tSkeleton.isOn;
    handsOn = tHands.isOn;
    gesturesOn = tGestures.isOn;
    depthVisMesh = tDepthVis.isOn;

	nuitrackModules.InitModules();
    nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);

    SwitchDepthVisualisation(tDepthMesh.isOn);
    SwitchBackground(tBackground.isOn);
  }

  Color[] backgroundColors = new Color[] {new Color (1f, 1f, 1f, 1f), new Color (1f, 1f, 1f, 0f)};
  int currentBGColor = 0;

  public void SwitchDepthVisualisation(bool meshEnabled)
  {
    depthVisMesh = meshEnabled;
    UserTrackerVisualization utv = FindObjectOfType<UserTrackerVisualization>();
    if (utv != null) utv.SetActive(!meshEnabled);
    
    UserTrackerVisMesh utvm = FindObjectOfType<UserTrackerVisMesh>();
    if (utvm != null) utvm.SetActive(meshEnabled);

    SwitchBackground(tBackground.isOn);
  }

  public void SwitchBackground(bool bgEnabled)
  {
    currentBGColor = bgEnabled ? 0 : 1;
    //currentBGColor = (currentBGColor + 1) % backgroundColors.Length;
    UserTrackerVisualization utv = FindObjectOfType<UserTrackerVisualization>();
    if (utv != null) utv.SetShaderProperties(backgroundColors[currentBGColor], bordersOn);

    UserTrackerVisMesh utvm = FindObjectOfType<UserTrackerVisMesh>();
    if (utvm != null) utvm.SetShaderProperties((currentBGColor == 0), bordersOn);
  }

  public void SwitchBorders()
  {
    bordersOn = !bordersOn;
    UserTrackerVisualization utv = FindObjectOfType<UserTrackerVisualization>();
    if (utv != null) utv.SetShaderProperties(backgroundColors[currentBGColor], bordersOn);

    UserTrackerVisMesh utvm = FindObjectOfType<UserTrackerVisMesh>();
    if (utvm != null) utvm.SetShaderProperties((currentBGColor == 0), bordersOn);

  }

  public void DepthToggle()
  {
    depthOn = tDepth.isOn;
    nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
  }

    public void ColorToggle()
    {
        colorOn = tColor.isOn;
        nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
    }

    public void UserToggle()
	{
		userOn = tUser.isOn;
    nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
	}

	public void SkeletonToggle()
	{
		skeletonOn = tSkeleton.isOn;
    nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
	}

	public void HandsToggle()
	{
		handsOn = tHands.isOn;
    nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
	}

	public void GesturesToggle()
	{
		gesturesOn = tGestures.isOn;
    nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
	}
}
