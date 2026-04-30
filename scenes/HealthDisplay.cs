using Godot;
using System;

public partial class HealthDisplay : Control
{
    [Export] public RichTextLabel nameLabel;
    [Export] public ProgressBar healthBar;
    private Minion _minion;

    public void Initialize(Minion minion)
    {
        _minion = minion;
        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay()
    {
        if (_minion == null) return;
        nameLabel.Text = _minion.name;
        healthBar.Value = (float)_minion.health / _minion.maxHealth * 100;
    }
}
