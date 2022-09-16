using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LumiSoft.Net.Log;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Debug;

public class wfrm_SIP_Debug : Form
{
	private TabControl m_pTab;

	private ToolStrip m_pTabLog_Toolbar;

	private RichTextBox m_pTabLog_Text;

	private ToolStrip m_pTabTransactions_Toolbar;

	private ListView m_pTabTransactions_List;

	private ToolStrip m_pTabDialogs_Toolbar;

	private ListView m_pTabDialogs_List;

	private ToolStrip m_pTabFlows_Toolbar;

	private ListView m_pTabFlows_List;

	private SIP_Stack m_pStack;

	private bool m_OddLogEntry;

	public wfrm_SIP_Debug(SIP_Stack stack)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		m_pStack = stack;
		m_pStack.Logger.WriteLog += Logger_WriteLog;
		InitUI();
	}

	private void InitUI()
	{
		base.ClientSize = new Size(600, 300);
		Text = "SIP Debug";
		base.FormClosed += wfrm_Debug_FormClosed;
		m_pTab = new TabControl();
		m_pTab.Dock = DockStyle.Fill;
		m_pTab.TabPages.Add("log", "Log");
		m_pTabLog_Toolbar = new ToolStrip();
		m_pTabLog_Toolbar.Dock = DockStyle.Top;
		m_pTab.TabPages["log"].Controls.Add(m_pTabLog_Toolbar);
		ToolStripButton tabLog_Toolbar_Log = new ToolStripButton("Log");
		tabLog_Toolbar_Log.Name = "log";
		tabLog_Toolbar_Log.Tag = "log";
		tabLog_Toolbar_Log.Checked = true;
		tabLog_Toolbar_Log.Click += delegate
		{
			tabLog_Toolbar_Log.Checked = !tabLog_Toolbar_Log.Checked;
		};
		m_pTabLog_Toolbar.Items.Add(tabLog_Toolbar_Log);
		ToolStripButton tabLog_Toolbar_LogData = new ToolStripButton("Log Data");
		tabLog_Toolbar_LogData.Name = "logdata";
		tabLog_Toolbar_LogData.Tag = "logdata";
		tabLog_Toolbar_LogData.Checked = true;
		tabLog_Toolbar_LogData.Click += delegate
		{
			tabLog_Toolbar_LogData.Checked = !tabLog_Toolbar_LogData.Checked;
		};
		m_pTabLog_Toolbar.Items.Add(tabLog_Toolbar_LogData);
		ToolStripButton toolStripButton = new ToolStripButton("Clear");
		toolStripButton.Tag = "clear";
		toolStripButton.Click += m_pTabLog_Toolbar_Click;
		m_pTabLog_Toolbar.Items.Add(toolStripButton);
		m_pTabLog_Toolbar.Items.Add(new ToolStripLabel("Filter:"));
		ToolStripTextBox toolStripTextBox = new ToolStripTextBox();
		toolStripTextBox.Name = "filter";
		toolStripTextBox.AutoSize = false;
		toolStripTextBox.Size = new Size(150, 20);
		m_pTabLog_Toolbar.Items.Add(toolStripTextBox);
		m_pTabLog_Text = new RichTextBox();
		m_pTabLog_Text.Size = new Size(m_pTab.TabPages["log"].Width, m_pTab.TabPages["log"].Height - 25);
		m_pTabLog_Text.Location = new Point(0, 25);
		m_pTabLog_Text.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m_pTabLog_Text.BorderStyle = BorderStyle.None;
		m_pTab.TabPages["log"].Controls.Add(m_pTabLog_Text);
		m_pTab.TabPages.Add("transactions", "Transactions");
		m_pTabTransactions_Toolbar = new ToolStrip();
		m_pTabTransactions_Toolbar.Dock = DockStyle.Top;
		ToolStripButton toolStripButton2 = new ToolStripButton("Refresh");
		toolStripButton2.Tag = "refresh";
		toolStripButton2.Click += m_pTabTransactions_Toolbar_Click;
		m_pTabTransactions_Toolbar.Items.Add(toolStripButton2);
		m_pTab.TabPages["transactions"].Controls.Add(m_pTabTransactions_Toolbar);
		m_pTabTransactions_List = new ListView();
		m_pTabTransactions_List.Size = new Size(m_pTab.TabPages["transactions"].Width, m_pTab.TabPages["transactions"].Height - 25);
		m_pTabTransactions_List.Location = new Point(0, 25);
		m_pTabTransactions_List.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m_pTabTransactions_List.View = View.Details;
		m_pTabTransactions_List.Columns.Add("Is Server");
		m_pTabTransactions_List.Columns.Add("Method", 80);
		m_pTabTransactions_List.Columns.Add("State", 80);
		m_pTabTransactions_List.Columns.Add("Create Time", 80);
		m_pTabTransactions_List.Columns.Add("ID", 100);
		m_pTab.TabPages["transactions"].Controls.Add(m_pTabTransactions_List);
		m_pTab.TabPages.Add("dialogs", "Dialogs");
		m_pTabDialogs_Toolbar = new ToolStrip();
		m_pTabDialogs_Toolbar.Dock = DockStyle.Top;
		ToolStripButton toolStripButton3 = new ToolStripButton("Refresh");
		toolStripButton3.Tag = "refresh";
		toolStripButton3.Click += m_pTabDialogs_Toolbar_Click;
		m_pTabDialogs_Toolbar.Items.Add(toolStripButton3);
		m_pTab.TabPages["dialogs"].Controls.Add(m_pTabDialogs_Toolbar);
		m_pTabDialogs_List = new ListView();
		m_pTabDialogs_List.Size = new Size(m_pTab.TabPages["dialogs"].Width, m_pTab.TabPages["dialogs"].Height - 25);
		m_pTabDialogs_List.Location = new Point(0, 25);
		m_pTabDialogs_List.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m_pTabDialogs_List.View = View.Details;
		m_pTabDialogs_List.Columns.Add("Type", 80);
		m_pTabDialogs_List.Columns.Add("State", 80);
		m_pTabDialogs_List.Columns.Add("Create Time", 100);
		m_pTabDialogs_List.Columns.Add("ID", 120);
		m_pTabDialogs_List.DoubleClick += m_pTabDialogs_List_DoubleClick;
		m_pTab.TabPages["dialogs"].Controls.Add(m_pTabDialogs_List);
		m_pTab.TabPages.Add("flows", "Flows");
		m_pTabFlows_Toolbar = new ToolStrip();
		m_pTabFlows_Toolbar.Dock = DockStyle.Top;
		ToolStripButton toolStripButton4 = new ToolStripButton("Refresh");
		toolStripButton4.Tag = "refresh";
		toolStripButton4.Click += m_pTabFlows_Toolbar_Click;
		m_pTabFlows_Toolbar.Items.Add(toolStripButton4);
		m_pTab.TabPages["flows"].Controls.Add(m_pTabFlows_Toolbar);
		m_pTabFlows_List = new ListView();
		m_pTabFlows_List.Size = new Size(m_pTab.TabPages["flows"].Width, m_pTab.TabPages["flows"].Height - 25);
		m_pTabFlows_List.Location = new Point(0, 25);
		m_pTabFlows_List.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m_pTabFlows_List.View = View.Details;
		m_pTabFlows_List.Columns.Add("Transport");
		m_pTabFlows_List.Columns.Add("Local EP", 130);
		m_pTabFlows_List.Columns.Add("Remote EP", 130);
		m_pTabFlows_List.Columns.Add("Last Activity", 80);
		m_pTabFlows_List.Columns.Add("Public EP", 130);
		m_pTab.TabPages["flows"].Controls.Add(m_pTabFlows_List);
		base.Controls.Add(m_pTab);
	}

	private void Logger_WriteLog(object sender, WriteLogEventArgs e)
	{
		if (!base.Visible)
		{
			return;
		}
		m_pTabLog_Text.BeginInvoke((MethodInvoker)delegate
		{
			if (((ToolStripButton)m_pTabLog_Toolbar.Items["log"]).Checked)
			{
				string text = e.LogEntry.Text + "\n";
				if (((ToolStripButton)m_pTabLog_Toolbar.Items["logdata"]).Checked && e.LogEntry.Data != null)
				{
					text = text + "<begin>\r\n" + Encoding.Default.GetString(e.LogEntry.Data) + "<end>\r\n";
				}
				if (IsAstericMatch(m_pTabLog_Toolbar.Items["filter"].Text, text))
				{
					if (m_OddLogEntry)
					{
						m_OddLogEntry = false;
						m_pTabLog_Text.SelectionColor = Color.Gray;
					}
					else
					{
						m_OddLogEntry = true;
						m_pTabLog_Text.SelectionColor = Color.LightSeaGreen;
					}
					m_pTabLog_Text.AppendText(text);
				}
			}
		});
	}

	private void m_pTabLog_Toolbar_Click(object sender, EventArgs e)
	{
		if (((ToolStripButton)sender).Tag.ToString() == "clear")
		{
			m_pTabLog_Text.Text = "";
		}
	}

	private void m_pTabTransactions_Toolbar_Click(object sender, EventArgs e)
	{
		if (!(((ToolStripButton)sender).Tag.ToString() == "refresh"))
		{
			return;
		}
		m_pTabTransactions_List.Items.Clear();
		SIP_ClientTransaction[] clientTransactions = m_pStack.TransactionLayer.ClientTransactions;
		foreach (SIP_ClientTransaction sIP_ClientTransaction in clientTransactions)
		{
			try
			{
				ListViewItem listViewItem = new ListViewItem("false");
				listViewItem.SubItems.Add(sIP_ClientTransaction.Method);
				listViewItem.SubItems.Add(sIP_ClientTransaction.State.ToString());
				listViewItem.SubItems.Add(sIP_ClientTransaction.CreateTime.ToString("HH:mm:ss"));
				listViewItem.SubItems.Add(sIP_ClientTransaction.ID);
				m_pTabTransactions_List.Items.Add(listViewItem);
			}
			catch
			{
			}
		}
		SIP_ServerTransaction[] serverTransactions = m_pStack.TransactionLayer.ServerTransactions;
		foreach (SIP_ServerTransaction sIP_ServerTransaction in serverTransactions)
		{
			try
			{
				ListViewItem listViewItem2 = new ListViewItem("true");
				listViewItem2.SubItems.Add(sIP_ServerTransaction.Method);
				listViewItem2.SubItems.Add(sIP_ServerTransaction.State.ToString());
				listViewItem2.SubItems.Add(sIP_ServerTransaction.CreateTime.ToString("HH:mm:ss"));
				listViewItem2.SubItems.Add(sIP_ServerTransaction.ID);
				m_pTabTransactions_List.Items.Add(listViewItem2);
			}
			catch
			{
			}
		}
	}

	private void m_pTabDialogs_Toolbar_Click(object sender, EventArgs e)
	{
		if (!(((ToolStripButton)sender).Tag.ToString() == "refresh"))
		{
			return;
		}
		m_pTabDialogs_List.Items.Clear();
		SIP_Dialog[] dialogs = m_pStack.TransactionLayer.Dialogs;
		foreach (SIP_Dialog sIP_Dialog in dialogs)
		{
			try
			{
				ListViewItem listViewItem = new ListViewItem((sIP_Dialog is SIP_Dialog_Invite) ? "INVITE" : "");
				listViewItem.SubItems.Add(sIP_Dialog.State.ToString());
				listViewItem.SubItems.Add(sIP_Dialog.CreateTime.ToString());
				listViewItem.SubItems.Add(sIP_Dialog.ID);
				listViewItem.Tag = sIP_Dialog;
				m_pTabDialogs_List.Items.Add(listViewItem);
			}
			catch
			{
			}
		}
	}

	private void m_pTabDialogs_List_DoubleClick(object sender, EventArgs e)
	{
		if (m_pTabDialogs_List.SelectedItems.Count != 0)
		{
			Form form = new Form();
			form.Size = new Size(400, 500);
			form.StartPosition = FormStartPosition.CenterScreen;
			form.Text = "Dialog Properties";
			PropertyGrid value = new PropertyGrid
			{
				Dock = DockStyle.Fill,
				SelectedObject = m_pTabDialogs_List.SelectedItems[0].Tag
			};
			form.Controls.Add(value);
			form.Show();
		}
	}

	private void m_pTabFlows_Toolbar_Click(object sender, EventArgs e)
	{
		if (!(((ToolStripButton)sender).Tag.ToString() == "refresh"))
		{
			return;
		}
		m_pTabFlows_List.Items.Clear();
		SIP_Flow[] flows = m_pStack.TransportLayer.Flows;
		foreach (SIP_Flow sIP_Flow in flows)
		{
			try
			{
				ListViewItem listViewItem = new ListViewItem(sIP_Flow.Transport);
				listViewItem.SubItems.Add(sIP_Flow.LocalEP.ToString());
				listViewItem.SubItems.Add(sIP_Flow.RemoteEP.ToString());
				listViewItem.SubItems.Add(sIP_Flow.LastActivity.ToString("HH:mm:ss"));
				listViewItem.SubItems.Add(sIP_Flow.LocalPublicEP.ToString());
				m_pTabFlows_List.Items.Add(listViewItem);
			}
			catch
			{
			}
		}
	}

	private void wfrm_Debug_FormClosed(object sender, FormClosedEventArgs e)
	{
		m_pStack.Logger.WriteLog -= Logger_WriteLog;
	}

	public static bool IsAstericMatch(string pattern, string text)
	{
		pattern = pattern.ToLower();
		text = text.ToLower();
		if (pattern == "")
		{
			pattern = "*";
		}
		while (pattern.Length > 0)
		{
			if (pattern.StartsWith("*"))
			{
				if (pattern.IndexOf("*", 1) > -1)
				{
					string text2 = pattern.Substring(1, pattern.IndexOf("*", 1) - 1);
					if (text.IndexOf(text2) == -1)
					{
						return false;
					}
					text = text.Substring(text.IndexOf(text2) + text2.Length);
					pattern = pattern.Substring(pattern.IndexOf("*", 1));
					continue;
				}
				return text.EndsWith(pattern.Substring(1));
			}
			if (pattern.IndexOfAny(new char[1] { '*' }) > -1)
			{
				string text3 = pattern.Substring(0, pattern.IndexOfAny(new char[1] { '*' }));
				if (!text.StartsWith(text3))
				{
					return false;
				}
				text = text.Substring(text.IndexOf(text3) + text3.Length);
				pattern = pattern.Substring(pattern.IndexOfAny(new char[1] { '*' }));
				continue;
			}
			return text == pattern;
		}
		return true;
	}
}
