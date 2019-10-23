namespace PerformanceViewer
{
    partial class Form1
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series13 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series14 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series15 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series16 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series17 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series18 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title3 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.label1 = new System.Windows.Forms.Label();
            this.listStationBox = new System.Windows.Forms.ListBox();
            this.chartStation = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.dataStatGridView = new System.Windows.Forms.DataGridView();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.dateTimeStartPicker = new System.Windows.Forms.DateTimePicker();
            this.dateTimeEndPicker = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chartStation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataStatGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Station";
            // 
            // listStationBox
            // 
            this.listStationBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.listStationBox.FormattingEnabled = true;
            this.listStationBox.Location = new System.Drawing.Point(12, 38);
            this.listStationBox.Name = "listStationBox";
            this.listStationBox.Size = new System.Drawing.Size(120, 654);
            this.listStationBox.TabIndex = 1;
            this.listStationBox.SelectedIndexChanged += new System.EventHandler(this.listStationBox_SelectedIndexChanged);
            // 
            // chartStation
            // 
            this.chartStation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            chartArea3.Name = "ChartArea1";
            this.chartStation.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.chartStation.Legends.Add(legend3);
            this.chartStation.Location = new System.Drawing.Point(138, 154);
            this.chartStation.Name = "chartStation";
            series13.ChartArea = "ChartArea1";
            series13.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series13.Legend = "Legend1";
            series13.MarkerColor = System.Drawing.Color.Red;
            series13.MarkerSize = 3;
            series13.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series13.Name = "Observed WL";
            series13.SmartLabelStyle.Enabled = false;
            series14.ChartArea = "ChartArea1";
            series14.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series14.Legend = "Legend1";
            series14.Name = "Day1 WL";
            series15.ChartArea = "ChartArea1";
            series15.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series15.Legend = "Legend1";
            series15.Name = "Day2 WL";
            series16.ChartArea = "ChartArea1";
            series16.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series16.Legend = "Legend1";
            series16.Name = "Day3 WL";
            series17.ChartArea = "ChartArea1";
            series17.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series17.Legend = "Legend1";
            series17.Name = "Day4 WL";
            series18.ChartArea = "ChartArea1";
            series18.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series18.Legend = "Legend1";
            series18.Name = "Day5 WL";
            this.chartStation.Series.Add(series13);
            this.chartStation.Series.Add(series14);
            this.chartStation.Series.Add(series15);
            this.chartStation.Series.Add(series16);
            this.chartStation.Series.Add(series17);
            this.chartStation.Series.Add(series18);
            this.chartStation.Size = new System.Drawing.Size(893, 538);
            this.chartStation.TabIndex = 2;
            this.chartStation.Text = "chartStation";
            title3.Name = "Title1";
            this.chartStation.Titles.Add(title3);
            // 
            // dataStatGridView
            // 
            this.dataStatGridView.AllowUserToAddRows = false;
            this.dataStatGridView.AllowUserToDeleteRows = false;
            this.dataStatGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dataStatGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataStatGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column3,
            this.Column1,
            this.Column2,
            this.Column4});
            this.dataStatGridView.Location = new System.Drawing.Point(444, 9);
            this.dataStatGridView.Name = "dataStatGridView";
            this.dataStatGridView.Size = new System.Drawing.Size(468, 139);
            this.dataStatGridView.TabIndex = 3;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Day";
            this.Column3.Name = "Column3";
            this.Column3.Width = 75;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "R^2";
            this.Column1.Name = "Column1";
            // 
            // Column2
            // 
            this.Column2.HeaderText = "MAE";
            this.Column2.Name = "Column2";
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Comment";
            this.Column4.Name = "Column4";
            this.Column4.Width = 150;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(919, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Export Table";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(918, 125);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(113, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Export Figure";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // dateTimeStartPicker
            // 
            this.dateTimeStartPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimeStartPicker.Location = new System.Drawing.Point(219, 58);
            this.dateTimeStartPicker.Name = "dateTimeStartPicker";
            this.dateTimeStartPicker.Size = new System.Drawing.Size(113, 20);
            this.dateTimeStartPicker.TabIndex = 6;
            // 
            // dateTimeEndPicker
            // 
            this.dateTimeEndPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimeEndPicker.Location = new System.Drawing.Point(219, 104);
            this.dateTimeEndPicker.Name = "dateTimeEndPicker";
            this.dateTimeEndPicker.Size = new System.Drawing.Size(113, 20);
            this.dateTimeEndPicker.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(170, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "From";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(170, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "To";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(202, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(161, 20);
            this.label4.TabIndex = 9;
            this.label4.Text = "Select Time Range";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 706);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dateTimeEndPicker);
            this.Controls.Add(this.dateTimeStartPicker);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataStatGridView);
            this.Controls.Add(this.chartStation);
            this.Controls.Add(this.listStationBox);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Performance Viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chartStation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataStatGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listStationBox;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartStation;
        private System.Windows.Forms.DataGridView dataStatGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DateTimePicker dateTimeStartPicker;
        private System.Windows.Forms.DateTimePicker dateTimeEndPicker;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}

