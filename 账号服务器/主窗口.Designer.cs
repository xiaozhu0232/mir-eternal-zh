namespace 账号服务器
{
	// Token: 0x02000002 RID: 2
	public partial class 主窗口 : global::System.Windows.Forms.Form
	{
		// Token: 0x06000014 RID: 20 RVA: 0x000026AB File Offset: 0x000008AB
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000026CC File Offset: 0x000008CC
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(主窗口));
            this.主选项卡 = new System.Windows.Forms.TabControl();
            this.日志选项卡 = new System.Windows.Forms.TabPage();
            this.日志文本框 = new System.Windows.Forms.RichTextBox();
            this.启动服务按钮 = new System.Windows.Forms.Button();
            this.停止服务按钮 = new System.Windows.Forms.Button();
            this.已注册账号 = new System.Windows.Forms.Label();
            this.新注册账号 = new System.Windows.Forms.Label();
            this.生成门票数 = new System.Windows.Forms.Label();
            this.已发送字节 = new System.Windows.Forms.Label();
            this.已接收字节 = new System.Windows.Forms.Label();
            this.本地监听端口 = new System.Windows.Forms.NumericUpDown();
            this.本地监听端口标签 = new System.Windows.Forms.Label();
            this.门票发送端口标签 = new System.Windows.Forms.Label();
            this.门票发送端口 = new System.Windows.Forms.NumericUpDown();
            this.最小化到托盘 = new System.Windows.Forms.NotifyIcon(this.components);
            this.托盘右键菜单 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.打开ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开配置按钮 = new System.Windows.Forms.Button();
            this.查看账号按钮 = new System.Windows.Forms.Button();
            this.加载配置按钮 = new System.Windows.Forms.Button();
            this.加载账号按钮 = new System.Windows.Forms.Button();
            this.主选项卡.SuspendLayout();
            this.日志选项卡.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.本地监听端口)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.门票发送端口)).BeginInit();
            this.托盘右键菜单.SuspendLayout();
            this.SuspendLayout();
            // 
            // 主选项卡
            // 
            this.主选项卡.Controls.Add(this.日志选项卡);
            this.主选项卡.ItemSize = new System.Drawing.Size(535, 22);
            this.主选项卡.Location = new System.Drawing.Point(0, 30);
            this.主选项卡.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.主选项卡.Name = "主选项卡";
            this.主选项卡.SelectedIndex = 0;
            this.主选项卡.Size = new System.Drawing.Size(540, 401);
            this.主选项卡.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.主选项卡.TabIndex = 0;
            // 
            // 日志选项卡
            // 
            this.日志选项卡.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.日志选项卡.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.日志选项卡.Controls.Add(this.日志文本框);
            this.日志选项卡.Location = new System.Drawing.Point(4, 26);
            this.日志选项卡.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.日志选项卡.Name = "日志选项卡";
            this.日志选项卡.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.日志选项卡.Size = new System.Drawing.Size(532, 371);
            this.日志选项卡.TabIndex = 0;
            this.日志选项卡.Text = "日志";
            // 
            // 日志文本框
            // 
            this.日志文本框.BackColor = System.Drawing.Color.Gainsboro;
            this.日志文本框.Location = new System.Drawing.Point(0, 0);
            this.日志文本框.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.日志文本框.Name = "日志文本框";
            this.日志文本框.ReadOnly = true;
            this.日志文本框.Size = new System.Drawing.Size(530, 366);
            this.日志文本框.TabIndex = 0;
            this.日志文本框.Text = "";
            // 
            // 启动服务按钮
            // 
            this.启动服务按钮.BackColor = System.Drawing.Color.YellowGreen;
            this.启动服务按钮.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.启动服务按钮.Location = new System.Drawing.Point(542, 286);
            this.启动服务按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.启动服务按钮.Name = "启动服务按钮";
            this.启动服务按钮.Size = new System.Drawing.Size(130, 73);
            this.启动服务按钮.TabIndex = 1;
            this.启动服务按钮.Text = "启动服务";
            this.启动服务按钮.UseVisualStyleBackColor = false;
            this.启动服务按钮.Click += new System.EventHandler(this.启动服务_Click);
            // 
            // 停止服务按钮
            // 
            this.停止服务按钮.BackColor = System.Drawing.Color.Crimson;
            this.停止服务按钮.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.停止服务按钮.Location = new System.Drawing.Point(541, 358);
            this.停止服务按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.停止服务按钮.Name = "停止服务按钮";
            this.停止服务按钮.Size = new System.Drawing.Size(130, 73);
            this.停止服务按钮.TabIndex = 2;
            this.停止服务按钮.Text = "停止服务";
            this.停止服务按钮.UseVisualStyleBackColor = false;
            this.停止服务按钮.Click += new System.EventHandler(this.停止服务_Click);
            // 
            // 已注册账号
            // 
            this.已注册账号.AutoSize = true;
            this.已注册账号.Location = new System.Drawing.Point(546, 40);
            this.已注册账号.Name = "已注册账号";
            this.已注册账号.Size = new System.Drawing.Size(83, 12);
            this.已注册账号.TabIndex = 3;
            this.已注册账号.Text = "已注册账号: 0";
            // 
            // 新注册账号
            // 
            this.新注册账号.AutoSize = true;
            this.新注册账号.Location = new System.Drawing.Point(546, 63);
            this.新注册账号.Name = "新注册账号";
            this.新注册账号.Size = new System.Drawing.Size(83, 12);
            this.新注册账号.TabIndex = 4;
            this.新注册账号.Text = "新注册账号: 0";
            // 
            // 生成门票数
            // 
            this.生成门票数.AutoSize = true;
            this.生成门票数.Location = new System.Drawing.Point(546, 86);
            this.生成门票数.Name = "生成门票数";
            this.生成门票数.Size = new System.Drawing.Size(83, 12);
            this.生成门票数.TabIndex = 5;
            this.生成门票数.Text = "生成门票数: 0";
            // 
            // 已发送字节
            // 
            this.已发送字节.AutoSize = true;
            this.已发送字节.Location = new System.Drawing.Point(546, 109);
            this.已发送字节.Name = "已发送字节";
            this.已发送字节.Size = new System.Drawing.Size(83, 12);
            this.已发送字节.TabIndex = 6;
            this.已发送字节.Text = "已发送字节: 0";
            // 
            // 已接收字节
            // 
            this.已接收字节.AutoSize = true;
            this.已接收字节.Location = new System.Drawing.Point(546, 132);
            this.已接收字节.Name = "已接收字节";
            this.已接收字节.Size = new System.Drawing.Size(83, 12);
            this.已接收字节.TabIndex = 7;
            this.已接收字节.Text = "已接收字节: 0";
            // 
            // 本地监听端口
            // 
            this.本地监听端口.Location = new System.Drawing.Point(90, 5);
            this.本地监听端口.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.本地监听端口.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.本地监听端口.Name = "本地监听端口";
            this.本地监听端口.Size = new System.Drawing.Size(87, 21);
            this.本地监听端口.TabIndex = 9;
            this.本地监听端口.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.本地监听端口.Value = new decimal(new int[] {
            8001,
            0,
            0,
            0});
            // 
            // 本地监听端口标签
            // 
            this.本地监听端口标签.AutoSize = true;
            this.本地监听端口标签.Location = new System.Drawing.Point(7, 9);
            this.本地监听端口标签.Name = "本地监听端口标签";
            this.本地监听端口标签.Size = new System.Drawing.Size(77, 12);
            this.本地监听端口标签.TabIndex = 8;
            this.本地监听端口标签.Text = "本地监听端口";
            // 
            // 门票发送端口标签
            // 
            this.门票发送端口标签.AutoSize = true;
            this.门票发送端口标签.Location = new System.Drawing.Point(197, 9);
            this.门票发送端口标签.Name = "门票发送端口标签";
            this.门票发送端口标签.Size = new System.Drawing.Size(77, 12);
            this.门票发送端口标签.TabIndex = 10;
            this.门票发送端口标签.Text = "门票发送端口";
            // 
            // 门票发送端口
            // 
            this.门票发送端口.Location = new System.Drawing.Point(280, 5);
            this.门票发送端口.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.门票发送端口.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.门票发送端口.Name = "门票发送端口";
            this.门票发送端口.Size = new System.Drawing.Size(87, 21);
            this.门票发送端口.TabIndex = 11;
            this.门票发送端口.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.门票发送端口.Value = new decimal(new int[] {
            6678,
            0,
            0,
            0});
            // 
            // 最小化到托盘
            // 
            this.最小化到托盘.ContextMenuStrip = this.托盘右键菜单;
            this.最小化到托盘.Icon = ((System.Drawing.Icon)(resources.GetObject("最小化到托盘.Icon")));
            this.最小化到托盘.Text = "账号服务器";
            this.最小化到托盘.MouseClick += new System.Windows.Forms.MouseEventHandler(this.恢复窗口_Click);
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
            this.打开ToolStripMenuItem.Click += new System.EventHandler(this.恢复窗口_Click);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.结束进程_Click);
            // 
            // 打开配置按钮
            // 
            this.打开配置按钮.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.打开配置按钮.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.打开配置按钮.Location = new System.Drawing.Point(541, 158);
            this.打开配置按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.打开配置按钮.Name = "打开配置按钮";
            this.打开配置按钮.Size = new System.Drawing.Size(130, 30);
            this.打开配置按钮.TabIndex = 12;
            this.打开配置按钮.Text = "打开服务器配置";
            this.打开配置按钮.UseVisualStyleBackColor = false;
            this.打开配置按钮.Click += new System.EventHandler(this.打开配置按钮_Click);
            // 
            // 查看账号按钮
            // 
            this.查看账号按钮.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.查看账号按钮.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.查看账号按钮.Location = new System.Drawing.Point(541, 222);
            this.查看账号按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.查看账号按钮.Name = "查看账号按钮";
            this.查看账号按钮.Size = new System.Drawing.Size(130, 30);
            this.查看账号按钮.TabIndex = 13;
            this.查看账号按钮.Text = "打开账号文件夹";
            this.查看账号按钮.UseVisualStyleBackColor = false;
            this.查看账号按钮.Click += new System.EventHandler(this.查看账号按钮_Click);
            // 
            // 加载配置按钮
            // 
            this.加载配置按钮.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.加载配置按钮.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.加载配置按钮.Location = new System.Drawing.Point(541, 190);
            this.加载配置按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.加载配置按钮.Name = "加载配置按钮";
            this.加载配置按钮.Size = new System.Drawing.Size(130, 30);
            this.加载配置按钮.TabIndex = 14;
            this.加载配置按钮.Text = "加载服务器配置";
            this.加载配置按钮.UseVisualStyleBackColor = false;
            this.加载配置按钮.Click += new System.EventHandler(this.加载配置按钮_Click);
            // 
            // 加载账号按钮
            // 
            this.加载账号按钮.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.加载账号按钮.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.加载账号按钮.Location = new System.Drawing.Point(542, 254);
            this.加载账号按钮.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.加载账号按钮.Name = "加载账号按钮";
            this.加载账号按钮.Size = new System.Drawing.Size(130, 30);
            this.加载账号按钮.TabIndex = 15;
            this.加载账号按钮.Text = "加载账号文件夹";
            this.加载账号按钮.UseVisualStyleBackColor = false;
            this.加载账号按钮.Click += new System.EventHandler(this.加载账号按钮_Click);
            // 
            // 主窗口
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 435);
            this.Controls.Add(this.加载账号按钮);
            this.Controls.Add(this.加载配置按钮);
            this.Controls.Add(this.查看账号按钮);
            this.Controls.Add(this.打开配置按钮);
            this.Controls.Add(this.门票发送端口标签);
            this.Controls.Add(this.门票发送端口);
            this.Controls.Add(this.本地监听端口标签);
            this.Controls.Add(this.本地监听端口);
            this.Controls.Add(this.已接收字节);
            this.Controls.Add(this.已发送字节);
            this.Controls.Add(this.生成门票数);
            this.Controls.Add(this.新注册账号);
            this.Controls.Add(this.已注册账号);
            this.Controls.Add(this.停止服务按钮);
            this.Controls.Add(this.启动服务按钮);
            this.Controls.Add(this.主选项卡);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "主窗口";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "账号服务器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.隐藏窗口_Click);
            this.Load += new System.EventHandler(this.主窗口_Load);
            this.主选项卡.ResumeLayout(false);
            this.日志选项卡.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.本地监听端口)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.门票发送端口)).EndInit();
            this.托盘右键菜单.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		// Token: 0x0400000A RID: 10
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400000B RID: 11
		private global::System.Windows.Forms.TabControl 主选项卡;

		// Token: 0x0400000C RID: 12
		private global::System.Windows.Forms.Button 启动服务按钮;

		// Token: 0x0400000D RID: 13
		private global::System.Windows.Forms.Button 停止服务按钮;

		// Token: 0x0400000E RID: 14
		private global::System.Windows.Forms.TabPage 日志选项卡;

		// Token: 0x0400000F RID: 15
		public global::System.Windows.Forms.RichTextBox 日志文本框;

		// Token: 0x04000010 RID: 16
		private global::System.Windows.Forms.Label 已注册账号;

		// Token: 0x04000011 RID: 17
		private global::System.Windows.Forms.Label 新注册账号;

		// Token: 0x04000012 RID: 18
		private global::System.Windows.Forms.Label 生成门票数;

		// Token: 0x04000013 RID: 19
		private global::System.Windows.Forms.Label 已发送字节;

		// Token: 0x04000014 RID: 20
		private global::System.Windows.Forms.Label 已接收字节;

		// Token: 0x04000015 RID: 21
		private global::System.Windows.Forms.Label 本地监听端口标签;

		// Token: 0x04000016 RID: 22
		private global::System.Windows.Forms.Label 门票发送端口标签;

		// Token: 0x04000017 RID: 23
		public global::System.Windows.Forms.NumericUpDown 本地监听端口;

		// Token: 0x04000018 RID: 24
		public global::System.Windows.Forms.NumericUpDown 门票发送端口;

		// Token: 0x04000019 RID: 25
		private global::System.Windows.Forms.NotifyIcon 最小化到托盘;

		// Token: 0x0400001A RID: 26
		private global::System.Windows.Forms.ContextMenuStrip 托盘右键菜单;

		// Token: 0x0400001B RID: 27
		private global::System.Windows.Forms.ToolStripMenuItem 打开ToolStripMenuItem;

		// Token: 0x0400001C RID: 28
		private global::System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;

		// Token: 0x0400001D RID: 29
		private global::System.Windows.Forms.Button 打开配置按钮;

		// Token: 0x0400001E RID: 30
		private global::System.Windows.Forms.Button 查看账号按钮;

		// Token: 0x0400001F RID: 31
		private global::System.Windows.Forms.Button 加载配置按钮;

		// Token: 0x04000020 RID: 32
		private global::System.Windows.Forms.Button 加载账号按钮;
	}
}
