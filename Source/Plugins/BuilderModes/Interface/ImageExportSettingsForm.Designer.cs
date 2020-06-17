﻿namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	partial class ImageExportSettingsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tbExportPath = new System.Windows.Forms.TextBox();
			this.browse = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.cancel = new System.Windows.Forms.Button();
			this.export = new System.Windows.Forms.Button();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.cbImageFormat = new System.Windows.Forms.ComboBox();
			this.cbPixelFormat = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.rbFloor = new System.Windows.Forms.RadioButton();
			this.rbCeiling = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// tbExportPath
			// 
			this.tbExportPath.Location = new System.Drawing.Point(50, 9);
			this.tbExportPath.Name = "tbExportPath";
			this.tbExportPath.Size = new System.Drawing.Size(344, 20);
			this.tbExportPath.TabIndex = 2;
			// 
			// browse
			// 
			this.browse.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.Folder;
			this.browse.Location = new System.Drawing.Point(400, 7);
			this.browse.Name = "browse";
			this.browse.Size = new System.Drawing.Size(30, 24);
			this.browse.TabIndex = 3;
			this.browse.UseVisualStyleBackColor = true;
			this.browse.Click += new System.EventHandler(this.browse_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Path:";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(360, 110);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(75, 23);
			this.cancel.TabIndex = 7;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// export
			// 
			this.export.Location = new System.Drawing.Point(279, 110);
			this.export.Name = "export";
			this.export.Size = new System.Drawing.Size(75, 23);
			this.export.TabIndex = 6;
			this.export.Text = "Export";
			this.export.UseVisualStyleBackColor = true;
			this.export.Click += new System.EventHandler(this.export_Click);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|All files (*.*)|*.*";
			// 
			// cbImageFormat
			// 
			this.cbImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbImageFormat.FormattingEnabled = true;
			this.cbImageFormat.Items.AddRange(new object[] {
            "PNG",
            "JPG"});
			this.cbImageFormat.Location = new System.Drawing.Point(102, 35);
			this.cbImageFormat.Name = "cbImageFormat";
			this.cbImageFormat.Size = new System.Drawing.Size(71, 21);
			this.cbImageFormat.TabIndex = 8;
			this.cbImageFormat.SelectedIndexChanged += new System.EventHandler(this.cbImageFormat_SelectedIndexChanged);
			// 
			// cbPixelFormat
			// 
			this.cbPixelFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbPixelFormat.FormattingEnabled = true;
			this.cbPixelFormat.Items.AddRange(new object[] {
            "32 bit",
            "24 bit",
            "16 bit"});
			this.cbPixelFormat.Location = new System.Drawing.Point(102, 62);
			this.cbPixelFormat.Name = "cbPixelFormat";
			this.cbPixelFormat.Size = new System.Drawing.Size(71, 21);
			this.cbPixelFormat.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(71, 13);
			this.label2.TabIndex = 10;
			this.label2.Text = "Image format:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 65);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 13);
			this.label3.TabIndex = 11;
			this.label3.Text = "Color depth:";
			// 
			// rbFloor
			// 
			this.rbFloor.AutoSize = true;
			this.rbFloor.Checked = true;
			this.rbFloor.Location = new System.Drawing.Point(227, 38);
			this.rbFloor.Name = "rbFloor";
			this.rbFloor.Size = new System.Drawing.Size(48, 17);
			this.rbFloor.TabIndex = 12;
			this.rbFloor.TabStop = true;
			this.rbFloor.Text = "Floor";
			this.rbFloor.UseVisualStyleBackColor = true;
			// 
			// rbCeiling
			// 
			this.rbCeiling.AutoSize = true;
			this.rbCeiling.Location = new System.Drawing.Point(227, 60);
			this.rbCeiling.Name = "rbCeiling";
			this.rbCeiling.Size = new System.Drawing.Size(56, 17);
			this.rbCeiling.TabIndex = 13;
			this.rbCeiling.Text = "Ceiling";
			this.rbCeiling.UseVisualStyleBackColor = true;
			// 
			// ImageExportSettingsForm
			// 
			this.AcceptButton = this.export;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(447, 145);
			this.Controls.Add(this.rbCeiling);
			this.Controls.Add(this.rbFloor);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cbPixelFormat);
			this.Controls.Add(this.cbImageFormat);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.export);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.browse);
			this.Controls.Add(this.tbExportPath);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImageExportSettingsForm";
			this.Text = "Image export settings";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button browse;
		private System.Windows.Forms.TextBox tbExportPath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button export;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.ComboBox cbImageFormat;
		private System.Windows.Forms.ComboBox cbPixelFormat;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RadioButton rbFloor;
		private System.Windows.Forms.RadioButton rbCeiling;
	}
}