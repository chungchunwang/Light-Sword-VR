using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class MultiplierMenuManager : MonoBehaviour
{
    PointSystem pointSystem;
    [SerializeField] TMP_Text multiplierLabel;
    [SerializeField] CircleProgressBarManager circleProgressBar;

    void OnEnable()
    {
        pointSystem = GameObject.FindGameObjectWithTag("Point System").GetComponent<PointSystem>();
        multiplierLabel.text = pointSystem.getCurrentMultiplier.ToString();
        circleProgressBar.setValue(pointSystem.getCurrentMultiplierUpgradePercentage());
    }
    private void Update()
    {
        if (pointSystem.getCurrentMultiplier.ToString().Equals(multiplierLabel.text) && pointSystem.getCurrentMultiplierUpgradePercentage() == circleProgressBar.getValue()) return;
        multiplierLabel.text = pointSystem.getCurrentMultiplier.ToString();
        circleProgressBar.setValue(pointSystem.getCurrentMultiplierUpgradePercentage());
    }
}
