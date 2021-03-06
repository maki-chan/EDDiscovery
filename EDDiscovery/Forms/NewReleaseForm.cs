﻿/*
 * Copyright © 2016 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using BaseUtils;
using BaseUtils.Win32;
using BaseUtils.Win32Constants;
using ExtendedControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDDiscovery.Forms
{
    public partial class NewReleaseForm : DraggableForm
    {
        private GitHubRelease _release = null;

        public NewReleaseForm(GitHubRelease release)
        {
            _release = release;
            InitializeComponent();
        }


        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _release = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            var framed = EDDTheme.Instance?.ApplyToForm(this) ?? true;
            if (framed)
            {
                // hide the caption panel, and resize the bottom panel to fit.
                var yoff = panel1.Location.Y - pnlCaption.Location.Y;
                pnlCaption.Visible = false;
                panel1.Location = pnlCaption.Location;
                panel1.Height += yoff;
            }
            else
            {
                // draw a thin border to serve as a resizing frame.
                pnlBack.BorderStyle = BorderStyle.FixedSingle;
            }

            if (_release != null)
            {
                textBoxReleaseName.Text = _release.ReleaseName;
                textBoxGitHubURL.Text = _release.HtmlURL;
                richTextBoxReleaseInfo.Text = _release.Description;

                if (string.IsNullOrEmpty(_release.ExeInstallerLink))
                    buttonExeInstaller.Visible = false;

                if (string.IsNullOrEmpty(_release.PortableInstallerLink))
                    buttonPortablezip.Visible = false;

                if (string.IsNullOrEmpty(_release.MsiInstallerLink))
                    buttonMsiInstaller.Visible = false;
            }

            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (pnlCaption.Visible)
            {
                if (this.WindowState == FormWindowState.Maximized && pnlMaxRestore.ImageSelected != DrawnPanel.ImageType.Restore)
                {
                    pnlMaxRestore.ImageSelected = DrawnPanel.ImageType.Restore;
                }
                else if (this.WindowState == FormWindowState.Normal && pnlMaxRestore.ImageSelected != DrawnPanel.ImageType.Maximize)
                {
                    pnlMaxRestore.ImageSelected = DrawnPanel.ImageType.Maximize;
                }   
            }

            base.OnResize(e);

            // Inhibit an issue where text box borders have artifacts displayed after a resize.
            // For some reason ControlStyles.ResizeRedraw, CS_VREDRAW, and/or CS_HREDRAW don't seem to fix it.
            if (this.FormBorderStyle == FormBorderStyle.None)
                Invalidate(true);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            lblCaption.Text = this.Text;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM.INITMENU:       // Win32: disable the minimize system menu item.
                    {
                        base.WndProc(ref m);    // Base should always get first crack at this.

                        if (m.WParam != IntPtr.Zero && Environment.OSVersion.Platform == PlatformID.Win32NT && IsHandleCreated)
                        {
                            UnsafeNativeMethods.EnableMenuItem(m.WParam, SC.MINIMIZE, MF.GRAYED);
                        }
                        return;
                    }

                case WM.SYSCOMMAND:     // Block the minimize system command.
                    {
                        if (m.WParam == (IntPtr)SC.MINIMIZE)
                        {
                            m.Result = IntPtr.Zero;
                            return;
                        }
                        break;
                    }
            }
            base.WndProc(ref m);
        }


        private void buttonUrlOpen_Click(object sender, EventArgs e)
        {
            Process.Start(_release.HtmlURL);
        }

        private void buttonExeInstaller_Click(object sender, EventArgs e)
        {
            Process.Start(_release.ExeInstallerLink);
        }

        private void buttonPortablezip_Click(object sender, EventArgs e)
        {
            Process.Start(_release.PortableInstallerLink);
        }

        private void buttonMsiInstaller_Click(object sender, EventArgs e)
        {
            Process.Start(_release.MsiInstallerLink);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        #region Caption controls

        private void Caption_MouseDown(object sender, MouseEventArgs e)
        {
            OnCaptionMouseDown((Control)sender, e);
        }

        private void Caption_MouseUp(object sender, MouseEventArgs e)
        {
            OnCaptionMouseUp((Control)sender, e);
        }

        private void pnlMaxRestore_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                switch (this.WindowState)
                {
                    case FormWindowState.Maximized:
                        this.WindowState = FormWindowState.Normal;
                        break;

                    case FormWindowState.Normal:
                        this.WindowState = FormWindowState.Maximized;
                        break;
                }
            }
            else if (e.Button == MouseButtons.Right)
                Caption_MouseUp(sender, e);
        }

        private void pnlClose_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Close();
            else if (e.Button == MouseButtons.Right)
                Caption_MouseUp(sender, e);
        }

        #endregion
    }
}
