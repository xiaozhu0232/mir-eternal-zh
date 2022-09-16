namespace 游戏登录器
{
	
	public partial class 登录界面 : global::System.Windows.Forms.Form
	{
		
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(登录界面));
            this.主选项卡 = new Sunny.UI.UITabControl();
            this.账号登录页面 = new System.Windows.Forms.TabPage();
            this.登录_用户图标 = new Sunny.UI.UIAvatar();
            this.登录_登录账号按钮 = new Sunny.UI.UISymbolButton();
            this.登录_忘记密码选项 = new Sunny.UI.UILinkLabel();
            this.登录_账号密码输入框 = new Sunny.UI.UITextBox();
            this.登录_注册账号按钮 = new Sunny.UI.UISymbolButton();
            this.登录_错误提示标签 = new Sunny.UI.UILabel();
            this.登录_账号名字输入框 = new Sunny.UI.UITextBox();
            this.账号注册页面 = new System.Windows.Forms.TabPage();
            this.注册_返回登录按钮 = new Sunny.UI.UISymbolButton();
            this.注册_错误提示标签 = new Sunny.UI.UILabel();
            this.注册_注册账号按钮 = new Sunny.UI.UISymbolButton();
            this.注册_密保答案输入框 = new Sunny.UI.UITextBox();
            this.注册_账号密码输入框 = new Sunny.UI.UITextBox();
            this.注册_密保问题输入框 = new Sunny.UI.UITextBox();
            this.注册_账号名字输入框 = new Sunny.UI.UITextBox();
            this.密码修改页面 = new System.Windows.Forms.TabPage();
            this.修改_返回登录按钮 = new Sunny.UI.UISymbolButton();
            this.修改_错误提示标签 = new Sunny.UI.UILabel();
            this.修改_修改密码按钮 = new Sunny.UI.UISymbolButton();
            this.修改_密保答案输入框 = new Sunny.UI.UITextBox();
            this.修改_账号密码输入框 = new Sunny.UI.UITextBox();
            this.修改_密保问题输入框 = new Sunny.UI.UITextBox();
            this.修改_账号名字输入框 = new Sunny.UI.UITextBox();
            this.启动游戏页面 = new System.Windows.Forms.TabPage();
            this.启动_当前账号标签 = new Sunny.UI.UISymbolButton();
            this.启动_当前选择标签 = new Sunny.UI.UILabel();
            this.启动_选中区服标签 = new Sunny.UI.UILinkLabel();
            this.启动_注销账号标签 = new Sunny.UI.UILinkLabel();
            this.启动_选择游戏区服 = new System.Windows.Forms.ListBox();
            this.启动_进入游戏按钮 = new Sunny.UI.UIButton();
            this.用户界面计时 = new System.Windows.Forms.Timer(this.components);
            this.数据处理计时 = new System.Windows.Forms.Timer(this.components);
            this.最小化到托盘 = new System.Windows.Forms.NotifyIcon(this.components);
            this.托盘右键菜单 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.打开ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.游戏进程监测 = new System.Windows.Forms.Timer(this.components);
            this.主选项卡.SuspendLayout();
            this.账号登录页面.SuspendLayout();
            this.账号注册页面.SuspendLayout();
            this.密码修改页面.SuspendLayout();
            this.启动游戏页面.SuspendLayout();
            this.托盘右键菜单.SuspendLayout();
            this.SuspendLayout();
            // 
            // 主选项卡
            // 
            this.主选项卡.Controls.Add(this.账号登录页面);
            this.主选项卡.Controls.Add(this.账号注册页面);
            this.主选项卡.Controls.Add(this.密码修改页面);
            this.主选项卡.Controls.Add(this.启动游戏页面);
            this.主选项卡.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.主选项卡.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(240)))));
            this.主选项卡.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.主选项卡.ItemSize = new System.Drawing.Size(260, 28);
            this.主选项卡.Location = new System.Drawing.Point(376, 0);
            this.主选项卡.MainPage = "";
            this.主选项卡.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.主选项卡.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.主选项卡.Name = "主选项卡";
            this.主选项卡.SelectedIndex = 0;
            this.主选项卡.Size = new System.Drawing.Size(331, 363);
            this.主选项卡.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.主选项卡.Style = Sunny.UI.UIStyle.LayuiRed;
            this.主选项卡.StyleCustomMode = true;
            this.主选项卡.TabIndex = 9;
            this.主选项卡.TabSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(56)))));
            this.主选项卡.TabSelectedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.主选项卡.TabSelectedHighColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.主选项卡.TabSelectedHighColorSize = 0;
            this.主选项卡.TabStop = false;
            this.主选项卡.TabUnSelectedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.主选项卡.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // 账号登录页面
            // 
            this.账号登录页面.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(240)))));
            this.账号登录页面.Controls.Add(this.登录_用户图标);
            this.账号登录页面.Controls.Add(this.登录_登录账号按钮);
            this.账号登录页面.Controls.Add(this.登录_忘记密码选项);
            this.账号登录页面.Controls.Add(this.登录_账号密码输入框);
            this.账号登录页面.Controls.Add(this.登录_注册账号按钮);
            this.账号登录页面.Controls.Add(this.登录_错误提示标签);
            this.账号登录页面.Controls.Add(this.登录_账号名字输入框);
            this.账号登录页面.Location = new System.Drawing.Point(0, 28);
            this.账号登录页面.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.账号登录页面.Name = "账号登录页面";
            this.账号登录页面.Size = new System.Drawing.Size(331, 335);
            this.账号登录页面.TabIndex = 0;
            this.账号登录页面.Text = "账号登录";
            // 
            // 登录_用户图标
            // 
            this.登录_用户图标.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.登录_用户图标.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_用户图标.Location = new System.Drawing.Point(104, 13);
            this.登录_用户图标.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.登录_用户图标.MinimumSize = new System.Drawing.Size(1, 1);
            this.登录_用户图标.Name = "登录_用户图标";
            this.登录_用户图标.Size = new System.Drawing.Size(45, 45);
            this.登录_用户图标.Style = Sunny.UI.UIStyle.Red;
            this.登录_用户图标.TabIndex = 10;
            this.登录_用户图标.TabStop = false;
            // 
            // 登录_登录账号按钮
            // 
            this.登录_登录账号按钮.Cursor = System.Windows.Forms.Cursors.Hand;
            this.登录_登录账号按钮.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_登录账号按钮.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_登录账号按钮.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_登录账号按钮.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_登录账号按钮.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_登录账号按钮.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.登录_登录账号按钮.Location = new System.Drawing.Point(21, 212);
            this.登录_登录账号按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.登录_登录账号按钮.MinimumSize = new System.Drawing.Size(1, 1);
            this.登录_登录账号按钮.Name = "登录_登录账号按钮";
            this.登录_登录账号按钮.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_登录账号按钮.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_登录账号按钮.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_登录账号按钮.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_登录账号按钮.Size = new System.Drawing.Size(217, 29);
            this.登录_登录账号按钮.Style = Sunny.UI.UIStyle.Red;
            this.登录_登录账号按钮.TabIndex = 13;
            this.登录_登录账号按钮.TabStop = false;
            this.登录_登录账号按钮.Text = "登录";
            this.登录_登录账号按钮.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.登录_登录账号按钮.Click += new System.EventHandler(this.登录_登录账号按钮_Click);
            // 
            // 登录_忘记密码选项
            // 
            this.登录_忘记密码选项.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(155)))), ((int)(((byte)(40)))));
            this.登录_忘记密码选项.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.登录_忘记密码选项.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.登录_忘记密码选项.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_忘记密码选项.Location = new System.Drawing.Point(173, 167);
            this.登录_忘记密码选项.Name = "登录_忘记密码选项";
            this.登录_忘记密码选项.Size = new System.Drawing.Size(65, 18);
            this.登录_忘记密码选项.Style = Sunny.UI.UIStyle.Red;
            this.登录_忘记密码选项.TabIndex = 16;
            this.登录_忘记密码选项.TabStop = true;
            this.登录_忘记密码选项.Text = "忘记密码?";
            this.登录_忘记密码选项.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_忘记密码选项.Click += new System.EventHandler(this.登录_忘记密码选项_Click);
            // 
            // 登录_账号密码输入框
            // 
            this.登录_账号密码输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号密码输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_账号密码输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_账号密码输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号密码输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_账号密码输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_账号密码输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.登录_账号密码输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.登录_账号密码输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.登录_账号密码输入框.Location = new System.Drawing.Point(21, 101);
            this.登录_账号密码输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.登录_账号密码输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.登录_账号密码输入框.Name = "登录_账号密码输入框";
            this.登录_账号密码输入框.PasswordChar = '*';
            this.登录_账号密码输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号密码输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号密码输入框.ShowText = false;
            this.登录_账号密码输入框.Size = new System.Drawing.Size(217, 29);
            this.登录_账号密码输入框.Style = Sunny.UI.UIStyle.Red;
            this.登录_账号密码输入框.Symbol = 61475;
            this.登录_账号密码输入框.SymbolSize = 22;
            this.登录_账号密码输入框.TabIndex = 2;
            this.登录_账号密码输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.登录_账号密码输入框.Watermark = "请输入密码";
            // 
            // 登录_注册账号按钮
            // 
            this.登录_注册账号按钮.Cursor = System.Windows.Forms.Cursors.Hand;
            this.登录_注册账号按钮.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_注册账号按钮.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_注册账号按钮.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_注册账号按钮.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_注册账号按钮.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_注册账号按钮.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.登录_注册账号按钮.Location = new System.Drawing.Point(21, 256);
            this.登录_注册账号按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.登录_注册账号按钮.MinimumSize = new System.Drawing.Size(1, 1);
            this.登录_注册账号按钮.Name = "登录_注册账号按钮";
            this.登录_注册账号按钮.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_注册账号按钮.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_注册账号按钮.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_注册账号按钮.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_注册账号按钮.Size = new System.Drawing.Size(217, 29);
            this.登录_注册账号按钮.Style = Sunny.UI.UIStyle.Red;
            this.登录_注册账号按钮.Symbol = 62004;
            this.登录_注册账号按钮.TabIndex = 14;
            this.登录_注册账号按钮.TabStop = false;
            this.登录_注册账号按钮.Text = "注册";
            this.登录_注册账号按钮.TipsColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.登录_注册账号按钮.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.登录_注册账号按钮.Click += new System.EventHandler(this.登录_注册账号按钮_Click);
            // 
            // 登录_错误提示标签
            // 
            this.登录_错误提示标签.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.登录_错误提示标签.ForeColor = System.Drawing.Color.Red;
            this.登录_错误提示标签.Location = new System.Drawing.Point(21, 194);
            this.登录_错误提示标签.Name = "登录_错误提示标签";
            this.登录_错误提示标签.Size = new System.Drawing.Size(169, 16);
            this.登录_错误提示标签.Style = Sunny.UI.UIStyle.Custom;
            this.登录_错误提示标签.TabIndex = 15;
            this.登录_错误提示标签.Text = "错误提示";
            this.登录_错误提示标签.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.登录_错误提示标签.Visible = false;
            // 
            // 登录_账号名字输入框
            // 
            this.登录_账号名字输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号名字输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_账号名字输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_账号名字输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号名字输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.登录_账号名字输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.登录_账号名字输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.登录_账号名字输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.登录_账号名字输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.登录_账号名字输入框.Location = new System.Drawing.Point(21, 64);
            this.登录_账号名字输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.登录_账号名字输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.登录_账号名字输入框.Name = "登录_账号名字输入框";
            this.登录_账号名字输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号名字输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.登录_账号名字输入框.ShowText = false;
            this.登录_账号名字输入框.Size = new System.Drawing.Size(217, 29);
            this.登录_账号名字输入框.Style = Sunny.UI.UIStyle.Red;
            this.登录_账号名字输入框.Symbol = 61447;
            this.登录_账号名字输入框.SymbolSize = 22;
            this.登录_账号名字输入框.TabIndex = 1;
            this.登录_账号名字输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.登录_账号名字输入框.Watermark = "请输入账号";
            // 
            // 账号注册页面
            // 
            this.账号注册页面.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(240)))));
            this.账号注册页面.Controls.Add(this.注册_返回登录按钮);
            this.账号注册页面.Controls.Add(this.注册_错误提示标签);
            this.账号注册页面.Controls.Add(this.注册_注册账号按钮);
            this.账号注册页面.Controls.Add(this.注册_密保答案输入框);
            this.账号注册页面.Controls.Add(this.注册_账号密码输入框);
            this.账号注册页面.Controls.Add(this.注册_密保问题输入框);
            this.账号注册页面.Controls.Add(this.注册_账号名字输入框);
            this.账号注册页面.Location = new System.Drawing.Point(0, 28);
            this.账号注册页面.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.账号注册页面.Name = "账号注册页面";
            this.账号注册页面.Size = new System.Drawing.Size(331, 335);
            this.账号注册页面.TabIndex = 1;
            this.账号注册页面.Text = "账号注册";
            // 
            // 注册_返回登录按钮
            // 
            this.注册_返回登录按钮.Cursor = System.Windows.Forms.Cursors.Hand;
            this.注册_返回登录按钮.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_返回登录按钮.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_返回登录按钮.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_返回登录按钮.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_返回登录按钮.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_返回登录按钮.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.注册_返回登录按钮.Location = new System.Drawing.Point(21, 263);
            this.注册_返回登录按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.注册_返回登录按钮.MinimumSize = new System.Drawing.Size(1, 1);
            this.注册_返回登录按钮.Name = "注册_返回登录按钮";
            this.注册_返回登录按钮.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_返回登录按钮.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_返回登录按钮.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_返回登录按钮.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_返回登录按钮.Size = new System.Drawing.Size(213, 30);
            this.注册_返回登录按钮.Style = Sunny.UI.UIStyle.Red;
            this.注册_返回登录按钮.Symbol = 61730;
            this.注册_返回登录按钮.TabIndex = 20;
            this.注册_返回登录按钮.TabStop = false;
            this.注册_返回登录按钮.Text = "返回登录";
            this.注册_返回登录按钮.TipsColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.注册_返回登录按钮.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.注册_返回登录按钮.Click += new System.EventHandler(this.注册_返回登录按钮_Click);
            // 
            // 注册_错误提示标签
            // 
            this.注册_错误提示标签.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.注册_错误提示标签.ForeColor = System.Drawing.Color.Red;
            this.注册_错误提示标签.Location = new System.Drawing.Point(21, 194);
            this.注册_错误提示标签.Name = "注册_错误提示标签";
            this.注册_错误提示标签.Size = new System.Drawing.Size(213, 23);
            this.注册_错误提示标签.Style = Sunny.UI.UIStyle.Custom;
            this.注册_错误提示标签.TabIndex = 17;
            this.注册_错误提示标签.Text = "错误提示";
            this.注册_错误提示标签.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.注册_错误提示标签.Visible = false;
            // 
            // 注册_注册账号按钮
            // 
            this.注册_注册账号按钮.Cursor = System.Windows.Forms.Cursors.Hand;
            this.注册_注册账号按钮.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_注册账号按钮.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_注册账号按钮.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_注册账号按钮.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_注册账号按钮.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_注册账号按钮.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.注册_注册账号按钮.Location = new System.Drawing.Point(21, 219);
            this.注册_注册账号按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.注册_注册账号按钮.MinimumSize = new System.Drawing.Size(1, 1);
            this.注册_注册账号按钮.Name = "注册_注册账号按钮";
            this.注册_注册账号按钮.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_注册账号按钮.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_注册账号按钮.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_注册账号按钮.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_注册账号按钮.Size = new System.Drawing.Size(213, 30);
            this.注册_注册账号按钮.Style = Sunny.UI.UIStyle.Red;
            this.注册_注册账号按钮.Symbol = 62004;
            this.注册_注册账号按钮.TabIndex = 16;
            this.注册_注册账号按钮.TabStop = false;
            this.注册_注册账号按钮.Text = "注册账号";
            this.注册_注册账号按钮.TipsColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.注册_注册账号按钮.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.注册_注册账号按钮.Click += new System.EventHandler(this.注册_注册账号按钮_Click);
            // 
            // 注册_密保答案输入框
            // 
            this.注册_密保答案输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保答案输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_密保答案输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_密保答案输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保答案输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_密保答案输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_密保答案输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.注册_密保答案输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.注册_密保答案输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.注册_密保答案输入框.Location = new System.Drawing.Point(21, 121);
            this.注册_密保答案输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.注册_密保答案输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.注册_密保答案输入框.Name = "注册_密保答案输入框";
            this.注册_密保答案输入框.PasswordChar = '*';
            this.注册_密保答案输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保答案输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保答案输入框.ShowText = false;
            this.注册_密保答案输入框.Size = new System.Drawing.Size(213, 30);
            this.注册_密保答案输入框.Style = Sunny.UI.UIStyle.Red;
            this.注册_密保答案输入框.Symbol = 61716;
            this.注册_密保答案输入框.SymbolSize = 22;
            this.注册_密保答案输入框.TabIndex = 4;
            this.注册_密保答案输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.注册_密保答案输入框.Watermark = "请输入密保答案";
            // 
            // 注册_账号密码输入框
            // 
            this.注册_账号密码输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号密码输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_账号密码输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_账号密码输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号密码输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_账号密码输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_账号密码输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.注册_账号密码输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.注册_账号密码输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.注册_账号密码输入框.Location = new System.Drawing.Point(21, 47);
            this.注册_账号密码输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.注册_账号密码输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.注册_账号密码输入框.Name = "注册_账号密码输入框";
            this.注册_账号密码输入框.PasswordChar = '*';
            this.注册_账号密码输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号密码输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号密码输入框.ShowText = false;
            this.注册_账号密码输入框.Size = new System.Drawing.Size(213, 30);
            this.注册_账号密码输入框.Style = Sunny.UI.UIStyle.Red;
            this.注册_账号密码输入框.Symbol = 61475;
            this.注册_账号密码输入框.SymbolSize = 22;
            this.注册_账号密码输入框.TabIndex = 2;
            this.注册_账号密码输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.注册_账号密码输入框.Watermark = "请输入密码";
            // 
            // 注册_密保问题输入框
            // 
            this.注册_密保问题输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保问题输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_密保问题输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_密保问题输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保问题输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_密保问题输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_密保问题输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.注册_密保问题输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.注册_密保问题输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.注册_密保问题输入框.Location = new System.Drawing.Point(21, 84);
            this.注册_密保问题输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.注册_密保问题输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.注册_密保问题输入框.Name = "注册_密保问题输入框";
            this.注册_密保问题输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保问题输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_密保问题输入框.ShowText = false;
            this.注册_密保问题输入框.Size = new System.Drawing.Size(213, 30);
            this.注册_密保问题输入框.Style = Sunny.UI.UIStyle.Red;
            this.注册_密保问题输入框.Symbol = 61563;
            this.注册_密保问题输入框.SymbolSize = 22;
            this.注册_密保问题输入框.TabIndex = 3;
            this.注册_密保问题输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.注册_密保问题输入框.Watermark = "请输入密保问题";
            // 
            // 注册_账号名字输入框
            // 
            this.注册_账号名字输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号名字输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_账号名字输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_账号名字输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号名字输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.注册_账号名字输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.注册_账号名字输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.注册_账号名字输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.注册_账号名字输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.注册_账号名字输入框.Location = new System.Drawing.Point(21, 10);
            this.注册_账号名字输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.注册_账号名字输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.注册_账号名字输入框.Name = "注册_账号名字输入框";
            this.注册_账号名字输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号名字输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.注册_账号名字输入框.ShowText = false;
            this.注册_账号名字输入框.Size = new System.Drawing.Size(213, 30);
            this.注册_账号名字输入框.Style = Sunny.UI.UIStyle.Red;
            this.注册_账号名字输入框.Symbol = 61447;
            this.注册_账号名字输入框.SymbolSize = 22;
            this.注册_账号名字输入框.TabIndex = 1;
            this.注册_账号名字输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.注册_账号名字输入框.Watermark = "请输入账号";
            // 
            // 密码修改页面
            // 
            this.密码修改页面.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(240)))));
            this.密码修改页面.Controls.Add(this.修改_返回登录按钮);
            this.密码修改页面.Controls.Add(this.修改_错误提示标签);
            this.密码修改页面.Controls.Add(this.修改_修改密码按钮);
            this.密码修改页面.Controls.Add(this.修改_密保答案输入框);
            this.密码修改页面.Controls.Add(this.修改_账号密码输入框);
            this.密码修改页面.Controls.Add(this.修改_密保问题输入框);
            this.密码修改页面.Controls.Add(this.修改_账号名字输入框);
            this.密码修改页面.Location = new System.Drawing.Point(0, 28);
            this.密码修改页面.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.密码修改页面.Name = "密码修改页面";
            this.密码修改页面.Size = new System.Drawing.Size(331, 335);
            this.密码修改页面.TabIndex = 2;
            this.密码修改页面.Text = "密码修改";
            // 
            // 修改_返回登录按钮
            // 
            this.修改_返回登录按钮.Cursor = System.Windows.Forms.Cursors.Hand;
            this.修改_返回登录按钮.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_返回登录按钮.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_返回登录按钮.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_返回登录按钮.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_返回登录按钮.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_返回登录按钮.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.修改_返回登录按钮.Location = new System.Drawing.Point(21, 262);
            this.修改_返回登录按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.修改_返回登录按钮.MinimumSize = new System.Drawing.Size(1, 1);
            this.修改_返回登录按钮.Name = "修改_返回登录按钮";
            this.修改_返回登录按钮.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_返回登录按钮.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_返回登录按钮.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_返回登录按钮.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_返回登录按钮.Size = new System.Drawing.Size(213, 30);
            this.修改_返回登录按钮.Style = Sunny.UI.UIStyle.Red;
            this.修改_返回登录按钮.Symbol = 61730;
            this.修改_返回登录按钮.TabIndex = 24;
            this.修改_返回登录按钮.TabStop = false;
            this.修改_返回登录按钮.Text = "返回登录";
            this.修改_返回登录按钮.TipsColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.修改_返回登录按钮.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.修改_返回登录按钮.Click += new System.EventHandler(this.修改_返回登录按钮_Click);
            // 
            // 修改_错误提示标签
            // 
            this.修改_错误提示标签.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.修改_错误提示标签.ForeColor = System.Drawing.Color.Red;
            this.修改_错误提示标签.Location = new System.Drawing.Point(18, 200);
            this.修改_错误提示标签.Name = "修改_错误提示标签";
            this.修改_错误提示标签.Size = new System.Drawing.Size(216, 16);
            this.修改_错误提示标签.Style = Sunny.UI.UIStyle.Custom;
            this.修改_错误提示标签.TabIndex = 22;
            this.修改_错误提示标签.Text = "错误提示";
            this.修改_错误提示标签.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.修改_错误提示标签.Visible = false;
            // 
            // 修改_修改密码按钮
            // 
            this.修改_修改密码按钮.Cursor = System.Windows.Forms.Cursors.Hand;
            this.修改_修改密码按钮.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_修改密码按钮.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_修改密码按钮.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_修改密码按钮.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_修改密码按钮.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_修改密码按钮.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.修改_修改密码按钮.Location = new System.Drawing.Point(21, 219);
            this.修改_修改密码按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.修改_修改密码按钮.MinimumSize = new System.Drawing.Size(1, 1);
            this.修改_修改密码按钮.Name = "修改_修改密码按钮";
            this.修改_修改密码按钮.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_修改密码按钮.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_修改密码按钮.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_修改密码按钮.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_修改密码按钮.Size = new System.Drawing.Size(213, 30);
            this.修改_修改密码按钮.Style = Sunny.UI.UIStyle.Red;
            this.修改_修改密码按钮.Symbol = 61573;
            this.修改_修改密码按钮.TabIndex = 21;
            this.修改_修改密码按钮.TabStop = false;
            this.修改_修改密码按钮.Text = "修改密码";
            this.修改_修改密码按钮.TipsColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.修改_修改密码按钮.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.修改_修改密码按钮.Click += new System.EventHandler(this.修改_修改密码按钮_Click);
            // 
            // 修改_密保答案输入框
            // 
            this.修改_密保答案输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保答案输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_密保答案输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_密保答案输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保答案输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_密保答案输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_密保答案输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.修改_密保答案输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.修改_密保答案输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.修改_密保答案输入框.Location = new System.Drawing.Point(21, 121);
            this.修改_密保答案输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.修改_密保答案输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.修改_密保答案输入框.Name = "修改_密保答案输入框";
            this.修改_密保答案输入框.PasswordChar = '*';
            this.修改_密保答案输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保答案输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保答案输入框.ShowText = false;
            this.修改_密保答案输入框.Size = new System.Drawing.Size(213, 30);
            this.修改_密保答案输入框.Style = Sunny.UI.UIStyle.Red;
            this.修改_密保答案输入框.Symbol = 61716;
            this.修改_密保答案输入框.SymbolSize = 22;
            this.修改_密保答案输入框.TabIndex = 20;
            this.修改_密保答案输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.修改_密保答案输入框.Watermark = "请输入密保答案";
            // 
            // 修改_账号密码输入框
            // 
            this.修改_账号密码输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号密码输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_账号密码输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_账号密码输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号密码输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_账号密码输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_账号密码输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.修改_账号密码输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.修改_账号密码输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.修改_账号密码输入框.Location = new System.Drawing.Point(21, 47);
            this.修改_账号密码输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.修改_账号密码输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.修改_账号密码输入框.Name = "修改_账号密码输入框";
            this.修改_账号密码输入框.PasswordChar = '*';
            this.修改_账号密码输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号密码输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号密码输入框.ShowText = false;
            this.修改_账号密码输入框.Size = new System.Drawing.Size(213, 30);
            this.修改_账号密码输入框.Style = Sunny.UI.UIStyle.Red;
            this.修改_账号密码输入框.Symbol = 61475;
            this.修改_账号密码输入框.SymbolSize = 22;
            this.修改_账号密码输入框.TabIndex = 18;
            this.修改_账号密码输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.修改_账号密码输入框.Watermark = "请输入新的密码";
            // 
            // 修改_密保问题输入框
            // 
            this.修改_密保问题输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保问题输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_密保问题输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_密保问题输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保问题输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_密保问题输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_密保问题输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.修改_密保问题输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.修改_密保问题输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.修改_密保问题输入框.Location = new System.Drawing.Point(21, 84);
            this.修改_密保问题输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.修改_密保问题输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.修改_密保问题输入框.Name = "修改_密保问题输入框";
            this.修改_密保问题输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保问题输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_密保问题输入框.ShowText = false;
            this.修改_密保问题输入框.Size = new System.Drawing.Size(213, 30);
            this.修改_密保问题输入框.Style = Sunny.UI.UIStyle.Red;
            this.修改_密保问题输入框.Symbol = 61563;
            this.修改_密保问题输入框.SymbolSize = 22;
            this.修改_密保问题输入框.TabIndex = 19;
            this.修改_密保问题输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.修改_密保问题输入框.Watermark = "请输入密保问题";
            // 
            // 修改_账号名字输入框
            // 
            this.修改_账号名字输入框.ButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号名字输入框.ButtonFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_账号名字输入框.ButtonFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_账号名字输入框.ButtonRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号名字输入框.ButtonRectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.修改_账号名字输入框.ButtonRectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.修改_账号名字输入框.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.修改_账号名字输入框.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.修改_账号名字输入框.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.修改_账号名字输入框.Location = new System.Drawing.Point(21, 10);
            this.修改_账号名字输入框.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.修改_账号名字输入框.MinimumSize = new System.Drawing.Size(1, 11);
            this.修改_账号名字输入框.Name = "修改_账号名字输入框";
            this.修改_账号名字输入框.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号名字输入框.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.修改_账号名字输入框.ShowText = false;
            this.修改_账号名字输入框.Size = new System.Drawing.Size(213, 30);
            this.修改_账号名字输入框.Style = Sunny.UI.UIStyle.Red;
            this.修改_账号名字输入框.Symbol = 61447;
            this.修改_账号名字输入框.SymbolSize = 22;
            this.修改_账号名字输入框.TabIndex = 17;
            this.修改_账号名字输入框.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.修改_账号名字输入框.Watermark = "请输入已有账号";
            // 
            // 启动游戏页面
            // 
            this.启动游戏页面.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(240)))));
            this.启动游戏页面.Controls.Add(this.启动_当前账号标签);
            this.启动游戏页面.Controls.Add(this.启动_当前选择标签);
            this.启动游戏页面.Controls.Add(this.启动_选中区服标签);
            this.启动游戏页面.Controls.Add(this.启动_注销账号标签);
            this.启动游戏页面.Controls.Add(this.启动_选择游戏区服);
            this.启动游戏页面.Controls.Add(this.启动_进入游戏按钮);
            this.启动游戏页面.Location = new System.Drawing.Point(0, 28);
            this.启动游戏页面.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.启动游戏页面.Name = "启动游戏页面";
            this.启动游戏页面.Size = new System.Drawing.Size(331, 335);
            this.启动游戏页面.TabIndex = 3;
            this.启动游戏页面.Text = "选择服务器";
            // 
            // 启动_当前账号标签
            // 
            this.启动_当前账号标签.Cursor = System.Windows.Forms.Cursors.Hand;
            this.启动_当前账号标签.Enabled = false;
            this.启动_当前账号标签.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.启动_当前账号标签.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.启动_当前账号标签.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(203)))), ((int)(((byte)(83)))));
            this.启动_当前账号标签.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.启动_当前账号标签.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.启动_当前账号标签.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.启动_当前账号标签.Location = new System.Drawing.Point(3, 2);
            this.启动_当前账号标签.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.启动_当前账号标签.MinimumSize = new System.Drawing.Size(1, 1);
            this.启动_当前账号标签.Name = "启动_当前账号标签";
            this.启动_当前账号标签.Radius = 15;
            this.启动_当前账号标签.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.启动_当前账号标签.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(203)))), ((int)(((byte)(83)))));
            this.启动_当前账号标签.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.启动_当前账号标签.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.启动_当前账号标签.Size = new System.Drawing.Size(253, 30);
            this.启动_当前账号标签.Style = Sunny.UI.UIStyle.Green;
            this.启动_当前账号标签.Symbol = 57607;
            this.启动_当前账号标签.TabIndex = 9;
            this.启动_当前账号标签.Text = "mistyes";
            this.启动_当前账号标签.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // 启动_当前选择标签
            // 
            this.启动_当前选择标签.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.启动_当前选择标签.Location = new System.Drawing.Point(7, 214);
            this.启动_当前选择标签.Name = "启动_当前选择标签";
            this.启动_当前选择标签.Size = new System.Drawing.Size(125, 20);
            this.启动_当前选择标签.TabIndex = 8;
            this.启动_当前选择标签.Text = "当前选择的服务器:";
            this.启动_当前选择标签.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // 启动_选中区服标签
            // 
            this.启动_选中区服标签.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(155)))), ((int)(((byte)(40)))));
            this.启动_选中区服标签.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.启动_选中区服标签.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.启动_选中区服标签.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.启动_选中区服标签.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.启动_选中区服标签.Location = new System.Drawing.Point(131, 214);
            this.启动_选中区服标签.Name = "启动_选中区服标签";
            this.启动_选中区服标签.Size = new System.Drawing.Size(116, 20);
            this.启动_选中区服标签.Style = Sunny.UI.UIStyle.Custom;
            this.启动_选中区服标签.TabIndex = 7;
            this.启动_选中区服标签.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.启动_选中区服标签.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            // 
            // 启动_注销账号标签
            // 
            this.启动_注销账号标签.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(155)))), ((int)(((byte)(40)))));
            this.启动_注销账号标签.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.启动_注销账号标签.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.启动_注销账号标签.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.启动_注销账号标签.LinkColor = System.Drawing.Color.Red;
            this.启动_注销账号标签.Location = new System.Drawing.Point(216, 34);
            this.启动_注销账号标签.Name = "启动_注销账号标签";
            this.启动_注销账号标签.Size = new System.Drawing.Size(40, 17);
            this.启动_注销账号标签.Style = Sunny.UI.UIStyle.Custom;
            this.启动_注销账号标签.TabIndex = 6;
            this.启动_注销账号标签.TabStop = true;
            this.启动_注销账号标签.Text = "退出";
            this.启动_注销账号标签.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.启动_注销账号标签.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.启动_注销账号标签.Click += new System.EventHandler(this.启动_注销账号标签_Click);
            // 
            // 启动_选择游戏区服
            // 
            this.启动_选择游戏区服.BackColor = System.Drawing.Color.Wheat;
            this.启动_选择游戏区服.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.启动_选择游戏区服.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.启动_选择游戏区服.ForeColor = System.Drawing.Color.Blue;
            this.启动_选择游戏区服.FormattingEnabled = true;
            this.启动_选择游戏区服.ItemHeight = 20;
            this.启动_选择游戏区服.Items.AddRange(new object[] {"魔龙谷","伤心树"});
            this.启动_选择游戏区服.Location = new System.Drawing.Point(73, 40);
            this.启动_选择游戏区服.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.启动_选择游戏区服.Name = "启动_选择游戏区服";
            this.启动_选择游戏区服.Size = new System.Drawing.Size(120, 171);
            this.启动_选择游戏区服.TabIndex = 4;
            this.启动_选择游戏区服.TabStop = false;
            this.启动_选择游戏区服.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.启动_选择游戏区服_DrawItem);
            this.启动_选择游戏区服.SelectedIndexChanged += new System.EventHandler(this.启动_选择游戏区服_SelectedIndexChanged);
            // 
            // 启动_进入游戏按钮
            // 
            this.启动_进入游戏按钮.Cursor = System.Windows.Forms.Cursors.Hand;
            this.启动_进入游戏按钮.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.启动_进入游戏按钮.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.启动_进入游戏按钮.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(121)))), ((int)(((byte)(78)))));
            this.启动_进入游戏按钮.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.启动_进入游戏按钮.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.启动_进入游戏按钮.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.启动_进入游戏按钮.Location = new System.Drawing.Point(3, 241);
            this.启动_进入游戏按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.启动_进入游戏按钮.MinimumSize = new System.Drawing.Size(1, 1);
            this.启动_进入游戏按钮.Name = "启动_进入游戏按钮";
            this.启动_进入游戏按钮.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(34)))));
            this.启动_进入游戏按钮.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(121)))), ((int)(((byte)(78)))));
            this.启动_进入游戏按钮.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.启动_进入游戏按钮.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(70)))), ((int)(((byte)(28)))));
            this.启动_进入游戏按钮.Size = new System.Drawing.Size(253, 30);
            this.启动_进入游戏按钮.Style = Sunny.UI.UIStyle.LayuiRed;
            this.启动_进入游戏按钮.TabIndex = 1;
            this.启动_进入游戏按钮.TabStop = false;
            this.启动_进入游戏按钮.Text = "进入游戏";
            this.启动_进入游戏按钮.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.启动_进入游戏按钮.Click += new System.EventHandler(this.启动_进入游戏按钮_Click);
            // 
            // 用户界面计时
            // 
            this.用户界面计时.Interval = 30000;
            this.用户界面计时.Tick += new System.EventHandler(this.用户界面解锁);
            // 
            // 数据处理计时
            // 
            this.数据处理计时.Enabled = true;
            this.数据处理计时.Tick += new System.EventHandler(this.数据接收处理);
            // 
            // 最小化到托盘
            // 
            this.最小化到托盘.ContextMenuStrip = this.托盘右键菜单;
            this.最小化到托盘.Icon = ((System.Drawing.Icon)(resources.GetObject("最小化到托盘.Icon")));
            this.最小化到托盘.Text = "登录器";
            this.最小化到托盘.MouseClick += new System.Windows.Forms.MouseEventHandler(this.托盘_恢复到任务栏);
            // 
            // 托盘右键菜单
            // 
            this.托盘右键菜单.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开ToolStripMenuItem,
            this.退出ToolStripMenuItem});
            this.托盘右键菜单.Name = "托盘右键菜单";
            this.托盘右键菜单.Size = new System.Drawing.Size(101, 48);
            // 
            // 打开ToolStripMenuItem
            // 
            this.打开ToolStripMenuItem.Name = "打开ToolStripMenuItem";
            this.打开ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.打开ToolStripMenuItem.Text = "打开";
            this.打开ToolStripMenuItem.Click += new System.EventHandler(this.托盘_恢复到任务栏);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.托盘_彻底关闭应用);
            // 
            // 游戏进程监测
            // 
            this.游戏进程监测.Enabled = true;
            this.游戏进程监测.Tick += new System.EventHandler(this.游戏进程检查);
            // 
            // 登录界面
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::游戏登录器.Properties.Resources.登录器;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(632, 361);
            this.Controls.Add(this.主选项卡);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "登录界面";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "游戏登录器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.托盘_隐藏到托盘区);
            this.主选项卡.ResumeLayout(false);
            this.账号登录页面.ResumeLayout(false);
            this.账号注册页面.ResumeLayout(false);
            this.密码修改页面.ResumeLayout(false);
            this.启动游戏页面.ResumeLayout(false);
            this.托盘右键菜单.ResumeLayout(false);
            this.ResumeLayout(false);

		}


		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.TabPage 账号登录页面;

		private global::Sunny.UI.UILinkLabel 登录_忘记密码选项;

		private global::Sunny.UI.UISymbolButton 登录_注册账号按钮;

		private global::Sunny.UI.UISymbolButton 登录_登录账号按钮;

		private global::Sunny.UI.UITextBox 登录_账号密码输入框;

		private global::Sunny.UI.UITextBox 登录_账号名字输入框;

		private global::Sunny.UI.UIAvatar 登录_用户图标;

		private global::System.Windows.Forms.TabPage 账号注册页面;

		private global::System.Windows.Forms.TabPage 密码修改页面;

		private global::System.Windows.Forms.TabPage 启动游戏页面;

		private global::Sunny.UI.UISymbolButton 注册_注册账号按钮;

		private global::Sunny.UI.UITextBox 注册_密保答案输入框;

		private global::Sunny.UI.UITextBox 注册_账号密码输入框;

		private global::Sunny.UI.UITextBox 注册_密保问题输入框;

		private global::Sunny.UI.UITextBox 注册_账号名字输入框;

		private global::Sunny.UI.UISymbolButton 修改_修改密码按钮;

		private global::Sunny.UI.UITextBox 修改_密保答案输入框;

		private global::Sunny.UI.UITextBox 修改_账号密码输入框;

		private global::Sunny.UI.UITextBox 修改_密保问题输入框;

		private global::Sunny.UI.UITextBox 修改_账号名字输入框;

		private global::Sunny.UI.UIButton 启动_进入游戏按钮;

		private global::Sunny.UI.UILabel 注册_错误提示标签;

		private global::Sunny.UI.UILabel 修改_错误提示标签;

		private global::System.Windows.Forms.ListBox 启动_选择游戏区服;

		private global::Sunny.UI.UILinkLabel 启动_选中区服标签;

		private global::Sunny.UI.UILinkLabel 启动_注销账号标签;

		private global::Sunny.UI.UISymbolButton 注册_返回登录按钮;

		private global::Sunny.UI.UISymbolButton 修改_返回登录按钮;

		private global::System.Windows.Forms.Timer 用户界面计时;

		public global::Sunny.UI.UITabControl 主选项卡;

		public global::Sunny.UI.UILabel 登录_错误提示标签;

		private global::System.Windows.Forms.Timer 数据处理计时;

		private global::Sunny.UI.UILabel 启动_当前选择标签;

		private global::Sunny.UI.UISymbolButton 启动_当前账号标签;

		private global::System.Windows.Forms.NotifyIcon 最小化到托盘;

		private global::System.Windows.Forms.ContextMenuStrip 托盘右键菜单;

		private global::System.Windows.Forms.ToolStripMenuItem 打开ToolStripMenuItem;

		private global::System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;

		private global::System.Windows.Forms.Timer 游戏进程监测;
	}
}
