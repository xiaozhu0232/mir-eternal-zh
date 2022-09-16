using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using LumiSoft.Net.Media.Codec;

namespace LumiSoft.Net.RTP.Debug;

public class wfrm_RTP_Debug : Form
{
	private class ComboBoxItem
	{
		private string m_Text = "";

		private object m_pTag;

		public string Text => m_Text;

		public object Tag => m_pTag;

		public ComboBoxItem(string text, object tag)
		{
			m_Text = text;
			m_pTag = tag;
		}

		public override string ToString()
		{
			return m_Text;
		}
	}

	private class RTP_SessionStatistics
	{
		private RTP_Session m_pSession;

		public long Members => m_pSession.Members.Length;

		public long Senders => m_pSession.Senders.Length;

		public long RtpPacketsSent => m_pSession.RtpPacketsSent;

		public long RtpBytesSent => m_pSession.RtpBytesSent;

		public long RtpPacketsReceived => m_pSession.RtpPacketsReceived;

		public long RtpBytesReceived => m_pSession.RtpBytesReceived;

		public long RtpFailedTransmissions => m_pSession.RtpFailedTransmissions;

		public long RtcpPacketsSent => m_pSession.RtcpPacketsSent;

		public long RtcpBytesSent => m_pSession.RtcpBytesSent;

		public long RtcpPacketsReceived => m_pSession.RtcpPacketsReceived;

		public long RtcpBytesReceived => m_pSession.RtcpBytesReceived;

		public long RtcpFailedTransmissions => m_pSession.RtcpFailedTransmissions;

		public int RtcpInterval => m_pSession.RtcpInterval;

		public string RtcpLastTransmission => m_pSession.RtcpLastTransmission.ToString("HH:mm:ss");

		public long LocalCollisions => m_pSession.LocalCollisions;

		public long RemoteCollisions => m_pSession.RemoteCollisions;

		public long LocalPacketsLooped => m_pSession.LocalPacketsLooped;

		public long RemotePacketsLooped => m_pSession.RemotePacketsLooped;

		public string Payload
		{
			get
			{
				int payload = m_pSession.Payload;
				Codec value = null;
				m_pSession.Payloads.TryGetValue(payload, out value);
				if (value == null)
				{
					return payload.ToString();
				}
				return payload + " - " + value.Name;
			}
		}

		public string[] Targets
		{
			get
			{
				List<string> list = new List<string>();
				RTP_Address[] targets = m_pSession.Targets;
				foreach (RTP_Address rTP_Address in targets)
				{
					list.Add(rTP_Address.IP?.ToString() + ":" + rTP_Address.DataPort + "/" + rTP_Address.ControlPort);
				}
				return list.ToArray();
			}
		}

		public string LocalEP => m_pSession.LocalEP.IP?.ToString() + ":" + m_pSession.LocalEP.DataPort + "/" + m_pSession.LocalEP.ControlPort;

		public string StreamMode => m_pSession.StreamMode.ToString();

		public RTP_SessionStatistics(RTP_Session session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			m_pSession = session;
		}
	}

	private class RTP_ParticipantInfo
	{
		private RTP_Participant m_pParticipant;

		public string Name
		{
			get
			{
				if (m_pParticipant is RTP_Participant_Local)
				{
					return ((RTP_Participant_Local)m_pParticipant).Name;
				}
				return ((RTP_Participant_Remote)m_pParticipant).Name;
			}
		}

		public string Email
		{
			get
			{
				if (m_pParticipant is RTP_Participant_Local)
				{
					return ((RTP_Participant_Local)m_pParticipant).Email;
				}
				return ((RTP_Participant_Remote)m_pParticipant).Email;
			}
		}

		public string Phone
		{
			get
			{
				if (m_pParticipant is RTP_Participant_Local)
				{
					return ((RTP_Participant_Local)m_pParticipant).Phone;
				}
				return ((RTP_Participant_Remote)m_pParticipant).Phone;
			}
		}

		public string Location
		{
			get
			{
				if (m_pParticipant is RTP_Participant_Local)
				{
					return ((RTP_Participant_Local)m_pParticipant).Location;
				}
				return ((RTP_Participant_Remote)m_pParticipant).Location;
			}
		}

		public string Tool
		{
			get
			{
				if (m_pParticipant is RTP_Participant_Local)
				{
					return ((RTP_Participant_Local)m_pParticipant).Tool;
				}
				return ((RTP_Participant_Remote)m_pParticipant).Tool;
			}
		}

		public string Note
		{
			get
			{
				if (m_pParticipant is RTP_Participant_Local)
				{
					return ((RTP_Participant_Local)m_pParticipant).Note;
				}
				return ((RTP_Participant_Remote)m_pParticipant).Note;
			}
		}

		public RTP_ParticipantInfo(RTP_Participant participant)
		{
			if (participant == null)
			{
				throw new ArgumentNullException("participant");
			}
			m_pParticipant = participant;
		}
	}

	private class RTP_SourceInfo
	{
		private RTP_Source m_pSource;

		public RTP_SourceState State => m_pSource.State;

		public int Session => m_pSource.Session.GetHashCode();

		public uint SSRC => m_pSource.SSRC;

		public IPEndPoint RtcpEP => m_pSource.RtcpEP;

		public IPEndPoint RtpEP => m_pSource.RtpEP;

		public string LastActivity => m_pSource.LastActivity.ToString("HH:mm:ss");

		public string LastRtcpPacket => m_pSource.LastRtcpPacket.ToString("HH:mm:ss");

		public string LastRtpPacket => m_pSource.LastRtpPacket.ToString("HH:mm:ss");

		public RTP_SourceInfo(RTP_Source source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			m_pSource = source;
		}
	}

	private class RTP_ReceiveStreamInfo
	{
		private RTP_ReceiveStream m_pStream;

		public int Session => m_pStream.Session.GetHashCode();

		public int SeqNoWrapCount => m_pStream.SeqNoWrapCount;

		public int FirstSeqNo => m_pStream.FirstSeqNo;

		public int MaxSeqNo => m_pStream.MaxSeqNo;

		public long PacketsReceived => m_pStream.PacketsReceived;

		public long PacketsMisorder => m_pStream.PacketsMisorder;

		public long BytesReceived => m_pStream.BytesReceived;

		public long PacketsLost => m_pStream.PacketsLost;

		public double Jitter => m_pStream.Jitter;

		public string LastSRTime => m_pStream.LastSRTime.ToString("HH:mm:ss");

		public int DelaySinceLastSR => m_pStream.DelaySinceLastSR / 1000;

		public RTP_ReceiveStreamInfo(RTP_ReceiveStream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pStream = stream;
		}
	}

	private class RTP_SendStreamInfo
	{
		private RTP_SendStream m_pStream;

		public int Session => m_pStream.Session.GetHashCode();

		public int SeqNoWrapCount => m_pStream.SeqNoWrapCount;

		public int SeqNo => m_pStream.SeqNo;

		public string LastPacketTime => m_pStream.LastPacketTime.ToString("HH:mm:ss");

		public uint LastPacketRtpTimestamp => m_pStream.LastPacketRtpTimestamp;

		public long RtpPacketsSent => m_pStream.RtpPacketsSent;

		public long RtpBytesSent => m_pStream.RtpBytesSent;

		public long RtpDataBytesSent => m_pStream.RtpDataBytesSent;

		public RTP_SendStreamInfo(RTP_SendStream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pStream = stream;
		}
	}

	private TabControl m_pTab;

	private SplitContainer m_pParticipantsSplitter;

	private TreeView m_pParticipants;

	private PropertyGrid m_pParticipantData;

	private ComboBox m_pSessions;

	private PropertyGrid m_pGlobalSessionInfo;

	private ListView m_pErrors;

	private bool m_IsDisposed;

	private RTP_MultimediaSession m_pSession;

	private Timer m_pTimer;

	public RTP_MultimediaSession Session => m_pSession;

	public wfrm_RTP_Debug(RTP_MultimediaSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		m_pSession = session;
		InitUI();
		base.Visible = true;
		m_pSession.Error += m_pSession_Error;
		m_pSession.SessionCreated += m_pSession_SessionCreated;
		m_pSession.NewParticipant += m_pSession_NewParticipant;
		m_pSession.LocalParticipant.SourceAdded += Participant_SourceAdded;
		m_pSession.LocalParticipant.SourceRemoved += Participant_SourceRemoved;
		m_pTimer = new Timer();
		m_pTimer.Interval = 1000;
		m_pTimer.Tick += m_pTimer_Tick;
		m_pTimer.Enabled = true;
		RTP_Session[] sessions = m_pSession.Sessions;
		foreach (RTP_Session rTP_Session in sessions)
		{
			ComboBoxItem item = new ComboBoxItem("Session: " + rTP_Session.GetHashCode(), new RTP_SessionStatistics(rTP_Session));
			m_pSessions.Items.Add(item);
		}
		if (m_pSessions.Items.Count > 0)
		{
			m_pSessions.SelectedIndex = 0;
		}
	}

	private void InitUI()
	{
		base.ClientSize = new Size(400, 500);
		Text = "RTP debug";
		base.FormClosing += wfrm_RTP_Debug_FormClosing;
		m_pTab = new TabControl();
		m_pTab.Dock = DockStyle.Fill;
		m_pTab.TabPages.Add("participants", "Participants");
		m_pParticipantsSplitter = new SplitContainer();
		m_pParticipantsSplitter.Dock = DockStyle.Fill;
		m_pParticipantsSplitter.Orientation = Orientation.Vertical;
		m_pParticipantsSplitter.SplitterDistance = 60;
		m_pTab.TabPages["participants"].Controls.Add(m_pParticipantsSplitter);
		m_pParticipants = new TreeView();
		m_pParticipants.Dock = DockStyle.Fill;
		m_pParticipants.BorderStyle = BorderStyle.None;
		m_pParticipants.FullRowSelect = true;
		m_pParticipants.HideSelection = false;
		m_pParticipants.AfterSelect += m_pParticipants_AfterSelect;
		TreeNode treeNode = new TreeNode(m_pSession.LocalParticipant.CNAME);
		treeNode.Tag = new RTP_ParticipantInfo(m_pSession.LocalParticipant);
		treeNode.Nodes.Add("Sources");
		m_pParticipants.Nodes.Add(treeNode);
		m_pParticipantsSplitter.Panel1.Controls.Add(m_pParticipants);
		m_pParticipantData = new PropertyGrid();
		m_pParticipantData.Dock = DockStyle.Fill;
		m_pParticipantsSplitter.Panel2.Controls.Add(m_pParticipantData);
		m_pTab.TabPages.Add("global_statistics", "Global statistics");
		m_pGlobalSessionInfo = new PropertyGrid();
		m_pGlobalSessionInfo.Dock = DockStyle.Fill;
		m_pTab.TabPages["global_statistics"].Controls.Add(m_pGlobalSessionInfo);
		m_pSessions = new ComboBox();
		m_pSessions.Size = new Size(200, 20);
		m_pSessions.Location = new Point(100, 2);
		m_pSessions.DropDownStyle = ComboBoxStyle.DropDownList;
		m_pSessions.SelectedIndexChanged += m_pSessions_SelectedIndexChanged;
		m_pTab.TabPages["global_statistics"].Controls.Add(m_pSessions);
		m_pSessions.BringToFront();
		m_pTab.TabPages.Add("errors", "Errors");
		m_pErrors = new ListView();
		m_pErrors.Dock = DockStyle.Fill;
		m_pErrors.View = View.Details;
		m_pErrors.FullRowSelect = true;
		m_pErrors.HideSelection = false;
		m_pErrors.Columns.Add("Message", 300);
		m_pErrors.DoubleClick += m_pErrors_DoubleClick;
		m_pTab.TabPages["errors"].Controls.Add(m_pErrors);
		base.Controls.Add(m_pTab);
	}

	private void m_pParticipants_AfterSelect(object sender, TreeViewEventArgs e)
	{
		m_pParticipantData.SelectedObject = e.Node.Tag;
	}

	private void m_pSessions_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_pGlobalSessionInfo.SelectedObject = ((ComboBoxItem)m_pSessions.SelectedItem).Tag;
	}

	private void m_pErrors_DoubleClick(object sender, EventArgs e)
	{
		if (m_pErrors.SelectedItems.Count > 0)
		{
			MessageBox.Show(this, "Error: " + ((Exception)m_pErrors.SelectedItems[0].Tag).ToString(), "Error:", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void m_pSession_Error(object sender, ExceptionEventArgs e)
	{
		if (!m_IsDisposed)
		{
			BeginInvoke((MethodInvoker)delegate
			{
				ListViewItem value = new ListViewItem(e.Exception.Message)
				{
					Tag = e.Exception
				};
				m_pErrors.Items.Add(value);
			});
		}
	}

	private void m_pSession_SessionCreated(object sender, EventArgs<RTP_Session> e)
	{
		if (m_IsDisposed)
		{
			return;
		}
		BeginInvoke((MethodInvoker)delegate
		{
			ComboBoxItem item = new ComboBoxItem("Session: " + e.Value.GetHashCode(), new RTP_SessionStatistics(e.Value));
			m_pSessions.Items.Add(item);
			if (m_pSessions.Items.Count > 0)
			{
				m_pSessions.SelectedIndex = 0;
			}
		});
	}

	private void m_pSession_NewParticipant(object sender, RTP_ParticipantEventArgs e)
	{
		if (!m_IsDisposed)
		{
			e.Participant.Removed += Participant_Removed;
			e.Participant.SourceAdded += Participant_SourceAdded;
			e.Participant.SourceRemoved += Participant_SourceRemoved;
			BeginInvoke((MethodInvoker)delegate
			{
				TreeNode node = new TreeNode(e.Participant.CNAME)
				{
					Tag = new RTP_ParticipantInfo(e.Participant),
					Nodes = { "Sources" }
				};
				m_pParticipants.Nodes.Add(node);
			});
		}
	}

	private void Participant_Removed(object sender, EventArgs e)
	{
		if (!m_IsDisposed)
		{
			BeginInvoke((MethodInvoker)delegate
			{
				FindParticipantNode((RTP_Participant)sender)?.Remove();
			});
		}
	}

	private void Participant_SourceAdded(object sender, RTP_SourceEventArgs e)
	{
		if (m_IsDisposed)
		{
			return;
		}
		e.Source.StateChanged += Source_StateChanged;
		BeginInvoke((MethodInvoker)delegate
		{
			TreeNode treeNode = null;
			treeNode = ((!(e.Source is RTP_Source_Remote)) ? FindParticipantNode(((RTP_Source_Local)e.Source).Participant) : FindParticipantNode(((RTP_Source_Remote)e.Source).Participant));
			TreeNode treeNode2 = treeNode.Nodes[0].Nodes.Add(e.Source.SSRC.ToString());
			treeNode2.Tag = new RTP_SourceInfo(e.Source);
			if (e.Source.State == RTP_SourceState.Active)
			{
				TreeNode treeNode3 = treeNode2.Nodes.Add("RTP Stream");
				if (e.Source is RTP_Source_Local)
				{
					treeNode3.Tag = new RTP_SendStreamInfo(((RTP_Source_Local)e.Source).Stream);
				}
				else
				{
					treeNode3.Tag = new RTP_ReceiveStreamInfo(((RTP_Source_Remote)e.Source).Stream);
				}
			}
		});
	}

	private void Source_StateChanged(object sender, EventArgs e)
	{
		if (m_IsDisposed)
		{
			return;
		}
		RTP_Source source = (RTP_Source)sender;
		if (source.State == RTP_SourceState.Disposed)
		{
			return;
		}
		BeginInvoke((MethodInvoker)delegate
		{
			TreeNode treeNode = null;
			treeNode = ((!(source is RTP_Source_Remote)) ? FindParticipantNode(((RTP_Source_Local)source).Participant) : FindParticipantNode(((RTP_Source_Remote)source).Participant));
			if (treeNode != null)
			{
				foreach (TreeNode node in treeNode.Nodes[0].Nodes)
				{
					if (node.Text == source.SSRC.ToString())
					{
						if (source.State == RTP_SourceState.Active)
						{
							TreeNode treeNode3 = node.Nodes.Add("RTP Stream");
							if (source is RTP_Source_Local)
							{
								treeNode3.Tag = new RTP_SendStreamInfo(((RTP_Source_Local)source).Stream);
							}
							else
							{
								treeNode3.Tag = new RTP_ReceiveStreamInfo(((RTP_Source_Remote)source).Stream);
							}
						}
						break;
					}
				}
			}
		});
	}

	private void Participant_SourceRemoved(object sender, RTP_SourceEventArgs e)
	{
		if (m_IsDisposed)
		{
			return;
		}
		uint ssrc = e.Source.SSRC;
		BeginInvoke((MethodInvoker)delegate
		{
			TreeNode treeNode = FindParticipantNode((RTP_Participant)sender);
			if (treeNode != null)
			{
				foreach (TreeNode node in treeNode.Nodes[0].Nodes)
				{
					if (node.Text == ssrc.ToString())
					{
						node.Remove();
						break;
					}
				}
			}
		});
	}

	private void m_pTimer_Tick(object sender, EventArgs e)
	{
		if (!m_IsDisposed)
		{
			if (m_pSession.IsDisposed)
			{
				base.Visible = false;
				return;
			}
			m_pParticipantData.Refresh();
			m_pGlobalSessionInfo.Refresh();
		}
	}

	private void wfrm_RTP_Debug_FormClosing(object sender, FormClosingEventArgs e)
	{
		m_IsDisposed = true;
		m_pSession.Error -= m_pSession_Error;
		m_pSession.SessionCreated -= m_pSession_SessionCreated;
		m_pSession.NewParticipant -= m_pSession_NewParticipant;
		m_pSession.LocalParticipant.SourceAdded -= Participant_SourceAdded;
		m_pSession.LocalParticipant.SourceRemoved -= Participant_SourceRemoved;
		m_pTimer.Dispose();
	}

	private TreeNode FindParticipantNode(RTP_Participant participant)
	{
		if (participant == null)
		{
			throw new ArgumentNullException("participant");
		}
		foreach (TreeNode node in m_pParticipants.Nodes)
		{
			if (node.Text == participant.CNAME)
			{
				return node;
			}
		}
		return null;
	}
}
