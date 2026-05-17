namespace HotelGuestEditor
{
    partial class Form2
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblIdentity;
        private System.Windows.Forms.TextBox txtIdentity;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DataGridView dgvResults;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblIdentity = new System.Windows.Forms.Label();
            txtIdentity = new System.Windows.Forms.TextBox();
            btnSearch = new System.Windows.Forms.Button();
            btnSelect = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            dgvResults = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            SuspendLayout();
            // 
            // lblIdentity
            // 
            lblIdentity.Location = new System.Drawing.Point(15, 18);
            lblIdentity.Name = "lblIdentity";
            lblIdentity.Size = new System.Drawing.Size(150, 23);
            lblIdentity.TabIndex = 0;
            lblIdentity.Text = "Identity / Passport No";
            lblIdentity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtIdentity
            // 
            txtIdentity.Location = new System.Drawing.Point(170, 18);
            txtIdentity.Name = "txtIdentity";
            txtIdentity.Size = new System.Drawing.Size(260, 23);
            txtIdentity.TabIndex = 1;
            txtIdentity.KeyDown += txtIdentity_KeyDown;
            // 
            // btnSearch
            // 
            btnSearch.Location = new System.Drawing.Point(445, 16);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new System.Drawing.Size(90, 28);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnSelect
            // 
            btnSelect.Location = new System.Drawing.Point(915, 16);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new System.Drawing.Size(90, 28);
            btnSelect.TabIndex = 3;
            btnSelect.Text = "Select";
            btnSelect.UseVisualStyleBackColor = true;
            btnSelect.Click += btnSelect_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(1015, 16);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(90, 28);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // dgvResults
            // 
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgvResults.BackgroundColor = System.Drawing.SystemColors.Window;
            dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Location = new System.Drawing.Point(15, 58);
            dgvResults.MultiSelect = false;
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvResults.Size = new System.Drawing.Size(1090, 545);
            dgvResults.TabIndex = 5;
            dgvResults.CellDoubleClick += dgvResults_CellDoubleClick;
            dgvResults.KeyDown += dgvResults_KeyDown;
            // 
            // Form2
            // 
            AcceptButton = btnSearch;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(1120, 620);
            Controls.Add(dgvResults);
            Controls.Add(btnCancel);
            Controls.Add(btnSelect);
            Controls.Add(btnSearch);
            Controls.Add(txtIdentity);
            Controls.Add(lblIdentity);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form2";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Search By Identity / Passport";
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
