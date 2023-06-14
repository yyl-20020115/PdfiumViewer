using System;
using System.Collections.Generic;
using System.Text;

namespace PdfiumViewer
{
    partial class PdfViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PdfViewer));
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.printButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.zoomInButton = new System.Windows.Forms.ToolStripButton();
            this.zoomOutButton = new System.Windows.Forms.ToolStripButton();
            this.container = new System.Windows.Forms.SplitContainer();
            this.bookmarks = new PdfiumViewer.NativeTreeView();
            this.renderer = new PdfiumViewer.PdfRenderer();
            this._toolStrip.SuspendLayout();
            this.container.Panel1.SuspendLayout();
            this.container.Panel2.SuspendLayout();
            this.container.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolStrip
            // 
            this._toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveButton,
            this.printButton,
            this.toolStripSeparator1,
            this.zoomInButton,
            this.zoomOutButton});
            resources.ApplyResources(this._toolStrip, "_toolStrip");
            this._toolStrip.Name = "_toolStrip";
            // 
            // _saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveButton.Image = global::PdfiumViewer.Properties.Resources.disk_blue;
            resources.ApplyResources(this.saveButton, "_saveButton");
            this.saveButton.Name = "_saveButton";
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // _printButton
            // 
            this.printButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printButton.Image = global::PdfiumViewer.Properties.Resources.printer;
            resources.ApplyResources(this.printButton, "_printButton");
            this.printButton.Name = "_printButton";
            this.printButton.Click += new System.EventHandler(this.PrintButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // _zoomInButton
            // 
            this.zoomInButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomInButton.Image = global::PdfiumViewer.Properties.Resources.zoom_in;
            resources.ApplyResources(this.zoomInButton, "_zoomInButton");
            this.zoomInButton.Name = "_zoomInButton";
            this.zoomInButton.Click += new System.EventHandler(this.ZoomInButton_Click);
            // 
            // _zoomOutButton
            // 
            this.zoomOutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomOutButton.Image = global::PdfiumViewer.Properties.Resources.zoom_out;
            resources.ApplyResources(this.zoomOutButton, "_zoomOutButton");
            this.zoomOutButton.Name = "_zoomOutButton";
            this.zoomOutButton.Click += new System.EventHandler(this.ZoomOutButton_Click);
            // 
            // _container
            // 
            resources.ApplyResources(this.container, "_container");
            this.container.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.container.Name = "_container";
            // 
            // _container.Panel1
            // 
            this.container.Panel1.Controls.Add(this.bookmarks);
            // 
            // _container.Panel2
            // 
            this.container.Panel2.Controls.Add(this.renderer);
            this.container.TabStop = false;
            // 
            // _bookmarks
            // 
            resources.ApplyResources(this.bookmarks, "_bookmarks");
            this.bookmarks.FullRowSelect = true;
            this.bookmarks.Name = "_bookmarks";
            this.bookmarks.ShowLines = false;
            this.bookmarks.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Bookmarks_AfterSelect);
            // 
            // _renderer
            // 
            this.renderer.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.renderer, "_renderer");
            this.renderer.Name = "_renderer";
            this.renderer.Page = 0;
            this.renderer.Rotation = PdfiumViewer.PdfRotation.Rotate0;
            this.renderer.ZoomMode = PdfiumViewer.PdfViewerZoomMode.FitHeight;
            this.renderer.LinkClick += new PdfiumViewer.LinkClickEventHandler(this.Renderer_LinkClick);
            // 
            // PdfViewer
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.container);
            this.Controls.Add(this._toolStrip);
            this.Name = "PdfViewer";
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this.container.Panel1.ResumeLayout(false);
            this.container.Panel2.ResumeLayout(false);
            this.container.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip _toolStrip;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ToolStripButton printButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton zoomInButton;
        private System.Windows.Forms.ToolStripButton zoomOutButton;
        private System.Windows.Forms.SplitContainer container;
        private NativeTreeView bookmarks;
        private PdfRenderer renderer;
    }
}
