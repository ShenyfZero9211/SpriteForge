using System.Diagnostics;

namespace SampleBrowser;

public partial class Form1 : Form
{
    private ListBox _listBox = null!;
    private Label _descriptionLabel = null!;
    private Button _runButton = null!;
    private Button _refreshButton = null!;
    private string _spriteLauncherDir = null!;
    private string _samplesDir = null!;

    public Form1()
    {
        InitializeComponent();
        SetupUI();
        FindProjectPaths();
        ScanSamples();
    }

    private void SetupUI()
    {
        this.Text = "SpriteForge - 示例浏览器";
        this.ClientSize = new System.Drawing.Size(500, 400);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        // 标题
        var titleLabel = new Label
        {
            Text = "SpriteForge - 示例浏览器",
            Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
            Location = new Point(20, 15),
            Size = new Size(460, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };
        this.Controls.Add(titleLabel);

        // 示例列表
        _listBox = new ListBox
        {
            Location = new Point(20, 55),
            Size = new Size(460, 200),
            Font = new Font("Microsoft YaHei", 10)
        };
        _listBox.SelectedIndexChanged += OnSelectionChanged;
        _listBox.DoubleClick += OnRunClicked;
        this.Controls.Add(_listBox);

        // 描述标签
        _descriptionLabel = new Label
        {
            Text = "请选择一个示例程序",
            Location = new Point(20, 265),
            Size = new Size(460, 50),
            Font = new Font("Microsoft YaHei", 9),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(5)
        };
        this.Controls.Add(_descriptionLabel);

        // 运行按钮
        _runButton = new Button
        {
            Text = "运行选中示例",
            Location = new Point(280, 330),
            Size = new Size(120, 35),
            Font = new Font("Microsoft YaHei", 10)
        };
        _runButton.Click += OnRunClicked;
        this.Controls.Add(_runButton);

        // 刷新按钮
        _refreshButton = new Button
        {
            Text = "刷新列表",
            Location = new Point(100, 330),
            Size = new Size(120, 35),
            Font = new Font("Microsoft YaHei", 10)
        };
        _refreshButton.Click += (_, _) => ScanSamples();
        this.Controls.Add(_refreshButton);
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
                return;
            }
            dir = dir.Parent;
        }

        // fallback: 尝试从当前工作目录
        var cwd = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (cwd != null)
        {
            var candidate = Path.Combine(cwd.FullName, "SpriteLauncher");
            if (Directory.Exists(candidate))
            {
                _spriteLauncherDir = candidate;
                _samplesDir = Path.Combine(candidate, "samples");
                return;
            }
            cwd = cwd.Parent;
        }

        MessageBox.Show("找不到 SpriteLauncher 项目目录，请确保 SampleBrowser 与 SpriteLauncher 在同一解决方案中。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        _samplesDir = "";
    }

    private void ScanSamples()
    {
        _listBox.Items.Clear();
        if (string.IsNullOrEmpty(_samplesDir) || !Directory.Exists(_samplesDir))
        {
            _descriptionLabel.Text = "无法找到示例目录";
            return;
        }

        var dirs = Directory.GetDirectories(_samplesDir).OrderBy(d => d).ToList();
        foreach (var dir in dirs)
        {
            var luaFile = Path.Combine(dir, "main.lua");
            if (!File.Exists(luaFile)) continue;

            var name = Path.GetFileName(dir);
            var description = ExtractDescription(luaFile);
            _listBox.Items.Add(new SampleItem(name, description, luaFile));
        }

        if (_listBox.Items.Count > 0)
        {
            _listBox.SelectedIndex = 0;
        }
    }

    private static string ExtractDescription(string luaFile)
    {
        var lines = File.ReadAllLines(luaFile);
        if (lines.Length > 0 && lines[0].TrimStart().StartsWith("--"))
        {
            return lines[0].TrimStart('-', ' ');
        }
        return "无描述";
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (_listBox.SelectedItem is SampleItem item)
        {
            _descriptionLabel.Text = $"[{item.Name}]\n{item.Description}";
            _runButton.Enabled = true;
        }
        else
        {
            _descriptionLabel.Text = "请选择一个示例程序";
            _runButton.Enabled = false;
        }
    }

    private void OnRunClicked(object? sender, EventArgs e)
    {
        if (_listBox.SelectedItem is not SampleItem item) return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{_spriteLauncherDir}\" -- \"{item.ScriptPath}\"",
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

    private record SampleItem(string Name, string Description, string ScriptPath)
    {
        public override string ToString() => Name;
    }
}
