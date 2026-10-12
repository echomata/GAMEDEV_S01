using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class KnockdownCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;

    private KnockdownTarget[] targets;
    private int knockedDown;
    private string displayTemplate;

    private void Start()
    {
        targets = FindObjectsByType<KnockdownTarget>(FindObjectsSortMode.None);

        foreach (KnockdownTarget target in targets)
            target.Initialize(this);

        if (counterText != null)
            displayTemplate = counterText.text;

        RefreshDisplay();
    }

    public void ReportKnockedDown(KnockdownTarget target)
    {
        knockedDown++;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (counterText == null)
            return;

        string template = string.IsNullOrEmpty(displayTemplate) ? "Cans Knocked:\n0" : displayTemplate;
        string replacement = knockedDown.ToString();

        template = template.Replace("{current}", knockedDown.ToString())
                           .Replace("{total}", string.Empty);

        if (template == displayTemplate)
        {
            string updatedTemplate = new Regex(@"\d+").Replace(template, replacement, 1);
            template = updatedTemplate == template ? $"{template}\n{replacement}" : updatedTemplate;
        }

        counterText.text = template;
    }
}
