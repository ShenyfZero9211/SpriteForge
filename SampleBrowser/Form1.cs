using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SampleBrowser;

public partial class Form1 : Form
{
    // Lua 页签控件
    private ListBox _luaListBox = null!;
    private Label _luaDescriptionLabel = null!;
    private Button _luaRunButton = null!;

    // C# 页签控件
    private ListBox _csListBox = null!;
    private Label _csDescriptionLabel = null!;
    private Button _csRunButton = null!;

    private string _spriteLauncherDir = null!;
    private string _samplesDir = null!;
    private string _csSamplesDir = null!;

    public Form1()
    {
        InitializeComponent();
        SetupUI();
        FindProjectPaths();
        ScanLuaSamples();
        ScanCsSketches();
    }

    private void SetupUI()
    {
        this.Text = "SpriteForge - 示例浏览器";
        this.ClientSize = new System.Drawing.Size(520, 440);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        // 标题
        var titleLabel = new Label
        {
            Text = "SpriteForge - 示例浏览器",
            Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
            Location = new Point(20, 12),
            Size = new Size(480, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };
        this.Controls.Add(titleLabel);

        // TabControl
        var tabControl = new TabControl
        {
            Location = new Point(20, 50),
            Size = new Size(480, 360),
            Font = new Font("Microsoft YaHei", 10)
        };
        this.Controls.Add(tabControl);

        // ── Lua 页签 ──
        var tabLua = new TabPage("Lua 示例");
        tabControl.TabPages.Add(tabLua);

        _luaListBox = new ListBox
        {
            Location = new Point(10, 10),
            Size = new Size(450, 190),
            Font = new Font("Microsoft YaHei", 10)
        };
        _luaListBox.SelectedIndexChanged += (_, _) => OnLuaSelectionChanged();
        _luaListBox.DoubleClick += (_, _) => OnLuaRunClicked();
        tabLua.Controls.Add(_luaListBox);

        _luaDescriptionLabel = new Label
        {
            Text = "请选择一个 Lua 示例",
            Location = new Point(10, 210),
            Size = new Size(450, 50),
            Font = new Font("Microsoft YaHei", 9),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(5)
        };
        tabLua.Controls.Add(_luaDescriptionLabel);

        var luaRefreshBtn = new Button
        {
            Text = "刷新列表",
            Location = new Point(90, 270),
            Size = new Size(110, 32),
            Font = new Font("Microsoft YaHei", 10)
        };
        luaRefreshBtn.Click += (_, _) => ScanLuaSamples();
        tabLua.Controls.Add(luaRefreshBtn);

        _luaRunButton = new Button
        {
            Text = "运行选中示例",
            Location = new Point(260, 270),
            Size = new Size(130, 32),
            Font = new Font("Microsoft YaHei", 10)
        };
        _luaRunButton.Click += (_, _) => OnLuaRunClicked();
        tabLua.Controls.Add(_luaRunButton);

        // ── C# 页签 ──
        var tabCs = new TabPage("C# Sketch");
        tabControl.TabPages.Add(tabCs);

        _csListBox = new ListBox
        {
            Location = new Point(10, 10),
            Size = new Size(450, 190),
            Font = new Font("Microsoft YaHei", 10)
        };
        _csListBox.SelectedIndexChanged += (_, _) => OnCsSelectionChanged();
        _csListBox.DoubleClick += (_, _) => OnCsRunClicked();
        tabCs.Controls.Add(_csListBox);

        _csDescriptionLabel = new Label
        {
            Text = "请选择一个 C# Sketch",
            Location = new Point(10, 210),
            Size = new Size(450, 50),
            Font = new Font("Microsoft YaHei", 9),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(5)
        };
        tabCs.Controls.Add(_csDescriptionLabel);

        var csRefreshBtn = new Button
        {
            Text = "刷新列表",
            Location = new Point(90, 270),
            Size = new Size(110, 32),
            Font = new Font("Microsoft YaHei", 10)
        };
        csRefreshBtn.Click += (_, _) => ScanCsSketches();
        tabCs.Controls.Add(csRefreshBtn);

        _csRunButton = new Button
        {
            Text = "运行选中 Sketch",
            Location = new Point(260, 270),
            Size = new Size(130, 32),
            Font = new Font("Microsoft YaHei", 10)
        };
        _csRunButton.Click += (_, _) => OnCsRunClicked();
        tabCs.Controls.Add(_csRunButton);
    }

    private void FindProjectPaths()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "SpriteLauncher");
            if (Directory.Exists(candidate))
            {
                _spriteLauncherDir = candidate;
                _samplesDir = Path.Combine(candidate, "samples");
                _csSamplesDir = Path.Combine(candidate, "cs-samples");
                return;
            }
            dir = dir.Parent;
        }

        var cwd = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (cwd != null)
        {
            var candidate = Path.Combine(cwd.FullName, "SpriteLauncher");
            if (Directory.Exists(candidate))
            {
                _spriteLauncherDir = candidate;
                _samplesDir = Path.Combine(candidate, "samples");
                _csSamplesDir = Path.Combine(candidate, "cs-samples");
                return;
            }
            cwd = cwd.Parent;
        }

        MessageBox.Show("找不到 SpriteLauncher 项目目录。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // ── Lua 示例 ──

    private void ScanLuaSamples()
    {
        _luaListBox.Items.Clear();
        if (string.IsNullOrEmpty(_samplesDir) || !Directory.Exists(_samplesDir))
        {
            _luaDescriptionLabel.Text = "无法找到 Lua 示例目录";
            return;
        }

        var dirs = Directory.GetDirectories(_samplesDir).OrderBy(d => d).ToList();
        foreach (var dir in dirs)
        {
            var luaFile = Path.Combine(dir, "main.lua");
            if (!File.Exists(luaFile)) continue;

            var name = Path.GetFileName(dir);
            var description = ExtractLuaDescription(luaFile);
            _luaListBox.Items.Add(new LuaItem(name, description, luaFile));
        }

        if (_luaListBox.Items.Count > 0)
            _luaListBox.SelectedIndex = 0;
    }

    private static string ExtractLuaDescription(string luaFile)
    {
        var lines = File.ReadAllLines(luaFile);
        if (lines.Length > 0 && lines[0].TrimStart().StartsWith("--"))
            return lines[0].TrimStart('-', ' ');
        return "无描述";
    }

    private void OnLuaSelectionChanged()
    {
        if (_luaListBox.SelectedItem is LuaItem item)
        {
            _luaDescriptionLabel.Text = $"[{item.Name}]\n{item.Description}";
            _luaRunButton.Enabled = true;
        }
        else
        {
            _luaDescriptionLabel.Text = "请选择一个 Lua 示例";
            _luaRunButton.Enabled = false;
        }
    }

    private void OnLuaRunClicked()
    {
        if (_luaListBox.SelectedItem is not LuaItem item) return;
        StartProcess($"run --project \"{_spriteLauncherDir}\" -- \"{item.ScriptPath}\"");
    }

    // ── C# Sketch ──

    private void ScanCsSketches()
    {
        _csListBox.Items.Clear();
        if (string.IsNullOrEmpty(_csSamplesDir) || !Directory.Exists(_csSamplesDir))
        {
            _csDescriptionLabel.Text = "无法找到 C# Sketch 目录";
            return;
        }

        var files = Directory.GetFiles(_csSamplesDir, "*.cs", SearchOption.AllDirectories).OrderBy(f => f).ToList();
        foreach (var file in files)
        {
            var info = ExtractSketchInfo(file);
            if (info == null) continue;

            _csListBox.Items.Add(new CsItem(info.Value.className, info.Value.description, file));
        }

        if (_csListBox.Items.Count > 0)
            _csListBox.SelectedIndex = 0;
    }

    private static (string className, string description)? ExtractSketchInfo(string csFile)
    {
        var content = File.ReadAllText(csFile);
        var match = Regex.Match(content, @"class\s+(\w+)\s*:\s*Sketch");
        if (!match.Success) return null;

        var className = match.Groups[1].Value;
        var classIndex = match.Index;
        var beforeClass = content.Substring(0, classIndex);

        // 尝试 /// <summary>
        var summaryMatch = Regex.Match(beforeClass, @"<summary>\s*(.*?)\s*</summary>", RegexOptions.Singleline);
        if (summaryMatch.Success)
        {
            var desc = string.Join("\n", summaryMatch.Groups[1].Value.Split('\n')
                .Select(l => l.Trim().TrimStart('/', ' ')));
            return (className, desc.Trim());
        }

        // 尝试 // 单行注释（从 class 定义往前找）
        var lines = beforeClass.Split('\n').Select(l => l.Trim()).ToList();
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;
            if (lines[i].StartsWith("//") && !lines[i].StartsWith("///"))
                return (className, lines[i].TrimStart('/', ' '));
            if (!lines[i].StartsWith("//"))
                break;
        }

        return (className, $"C# Sketch: {className}");
    }

    private void OnCsSelectionChanged()
    {
        if (_csListBox.SelectedItem is CsItem item)
        {
            _csDescriptionLabel.Text = $"[{item.ClassName}]\n{item.Description}";
            _csRunButton.Enabled = true;
        }
        else
        {
            _csDescriptionLabel.Text = "请选择一个 C# Sketch";
            _csRunButton.Enabled = false;
        }
    }

    private void OnCsRunClicked()
    {
        if (_csListBox.SelectedItem is not CsItem item) return;
        StartProcess($"run --project \"{_spriteLauncherDir}\" -- --sketch {item.ClassName}");
    }

    // ── 公共 ──

    private static void StartProcess(string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            using var process = Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private record LuaItem(string Name, string Description, string ScriptPath)
    {
        public override string ToString() => Name;
    }

    private record CsItem(string ClassName, string Description, string FilePath)
    {
        public override string ToString() => ClassName;
    }
}
