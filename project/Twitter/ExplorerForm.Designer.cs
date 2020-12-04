namespace Twitter
{
    partial class ExplorerForm
    {
        private System.ComponentModel.IContainer components = null;
        
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose( );
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tweetsListView = new System.Windows.Forms.ListView();
            this.tweetsListColumn_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tweetsListColumn_2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectSearchComboBox = new MetroFramework.Controls.MetroComboBox();
            this.panelLabel_1 = new MetroFramework.Controls.MetroLabel();
            this.infoLabel_1 = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // tweetsListView
            // 
            this.tweetsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tweetsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.tweetsListColumn_1,
            this.tweetsListColumn_2});
            this.tweetsListView.GridLines = true;
            this.tweetsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.tweetsListView.LabelWrap = false;
            this.tweetsListView.Location = new System.Drawing.Point(23, 104);
            this.tweetsListView.MultiSelect = false;
            this.tweetsListView.Name = "tweetsListView";
            this.tweetsListView.ShowGroups = false;
            this.tweetsListView.Size = new System.Drawing.Size(500, 173);
            this.tweetsListView.TabIndex = 128;
            this.tweetsListView.UseCompatibleStateImageBehavior = false;
            this.tweetsListView.View = System.Windows.Forms.View.Details;
            // 
            // tweetsListColumn_1
            // 
            this.tweetsListColumn_1.Text = "Creador";
            this.tweetsListColumn_1.Width = 100;
            // 
            // tweetsListColumn_2
            // 
            this.tweetsListColumn_2.Text = "Mensaje";
            this.tweetsListColumn_2.Width = 800;
            // 
            // selectSearchComboBox
            // 
            this.selectSearchComboBox.FormattingEnabled = true;
            this.selectSearchComboBox.ItemHeight = 23;
            this.selectSearchComboBox.Location = new System.Drawing.Point(338, 48);
            this.selectSearchComboBox.Name = "selectSearchComboBox";
            this.selectSearchComboBox.Size = new System.Drawing.Size(170, 29);
            this.selectSearchComboBox.Style = MetroFramework.MetroColorStyle.Blue;
            this.selectSearchComboBox.TabIndex = 129;
            this.selectSearchComboBox.UseSelectable = true;
            this.selectSearchComboBox.SelectedIndexChanged += new System.EventHandler(this.selectSearchComboBox_SelectedIndexChanged);
            // 
            // panelLabel_1
            // 
            this.panelLabel_1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLabel_1.Location = new System.Drawing.Point(23, 35);
            this.panelLabel_1.Name = "panelLabel_1";
            this.panelLabel_1.Size = new System.Drawing.Size(500, 55);
            this.panelLabel_1.Style = MetroFramework.MetroColorStyle.Blue;
            this.panelLabel_1.TabIndex = 130;
            this.panelLabel_1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // infoLabel_1
            // 
            this.infoLabel_1.Location = new System.Drawing.Point(38, 47);
            this.infoLabel_1.Name = "infoLabel_1";
            this.infoLabel_1.Size = new System.Drawing.Size(294, 30);
            this.infoLabel_1.Style = MetroFramework.MetroColorStyle.Blue;
            this.infoLabel_1.TabIndex = 131;
            this.infoLabel_1.Text = "Seleccionar la búsqueda que se desea explorar:";
            this.infoLabel_1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackImagePadding = new System.Windows.Forms.Padding(210, 3, 0, 0);
            this.BackMaxSize = 200;
            this.BorderStyle = MetroFramework.Forms.MetroFormBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(546, 296);
            this.Controls.Add(this.infoLabel_1);
            this.Controls.Add(this.selectSearchComboBox);
            this.Controls.Add(this.panelLabel_1);
            this.Controls.Add(this.tweetsListView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExplorerForm";
            this.Resizable = false;
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.DropShadow;
            this.Theme = MetroFramework.MetroThemeStyle.Default;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView tweetsListView;
        private System.Windows.Forms.ColumnHeader tweetsListColumn_1;
        private System.Windows.Forms.ColumnHeader tweetsListColumn_2;
        private MetroFramework.Controls.MetroComboBox selectSearchComboBox;
        private MetroFramework.Controls.MetroLabel panelLabel_1;
        private MetroFramework.Controls.MetroLabel infoLabel_1;
    }
}