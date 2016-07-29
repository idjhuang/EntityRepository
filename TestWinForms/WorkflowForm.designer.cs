namespace TestWinForms
{
    partial class WorkflowForm
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
            this.workflowList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.taskList = new System.Windows.Forms.ComboBox();
            this.execBtn = new System.Windows.Forms.Button();
            this.workflowName = new System.Windows.Forms.TextBox();
            this.createBtn = new System.Windows.Forms.Button();
            this.abortBtn = new System.Windows.Forms.Button();
            this.doneBtn = new System.Windows.Forms.Button();
            this.parameter = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // workflowList
            // 
            this.workflowList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.workflowList.FormattingEnabled = true;
            this.workflowList.Location = new System.Drawing.Point(83, 10);
            this.workflowList.Name = "workflowList";
            this.workflowList.Size = new System.Drawing.Size(189, 20);
            this.workflowList.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Workflows";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Tasks";
            // 
            // taskList
            // 
            this.taskList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.taskList.FormattingEnabled = true;
            this.taskList.Location = new System.Drawing.Point(83, 45);
            this.taskList.Name = "taskList";
            this.taskList.Size = new System.Drawing.Size(189, 20);
            this.taskList.TabIndex = 0;
            // 
            // execBtn
            // 
            this.execBtn.Location = new System.Drawing.Point(197, 71);
            this.execBtn.Name = "execBtn";
            this.execBtn.Size = new System.Drawing.Size(75, 23);
            this.execBtn.TabIndex = 2;
            this.execBtn.Text = "Execute Task";
            this.execBtn.UseVisualStyleBackColor = true;
            this.execBtn.Click += new System.EventHandler(this.execBtn_Click);
            // 
            // workflowName
            // 
            this.workflowName.Location = new System.Drawing.Point(12, 158);
            this.workflowName.Name = "workflowName";
            this.workflowName.Size = new System.Drawing.Size(179, 22);
            this.workflowName.TabIndex = 3;
            // 
            // createBtn
            // 
            this.createBtn.Location = new System.Drawing.Point(197, 158);
            this.createBtn.Name = "createBtn";
            this.createBtn.Size = new System.Drawing.Size(75, 23);
            this.createBtn.TabIndex = 2;
            this.createBtn.Text = "Create";
            this.createBtn.UseVisualStyleBackColor = true;
            this.createBtn.Click += new System.EventHandler(this.createBtn_Click);
            // 
            // abortBtn
            // 
            this.abortBtn.Location = new System.Drawing.Point(197, 129);
            this.abortBtn.Name = "abortBtn";
            this.abortBtn.Size = new System.Drawing.Size(75, 23);
            this.abortBtn.TabIndex = 2;
            this.abortBtn.Text = "Abort";
            this.abortBtn.UseVisualStyleBackColor = true;
            this.abortBtn.Click += new System.EventHandler(this.abortBtn_Click);
            // 
            // doneBtn
            // 
            this.doneBtn.Location = new System.Drawing.Point(197, 100);
            this.doneBtn.Name = "doneBtn";
            this.doneBtn.Size = new System.Drawing.Size(75, 23);
            this.doneBtn.TabIndex = 2;
            this.doneBtn.Text = "Done";
            this.doneBtn.UseVisualStyleBackColor = true;
            this.doneBtn.Click += new System.EventHandler(this.doneBtn_Click);
            // 
            // parameter
            // 
            this.parameter.Location = new System.Drawing.Point(12, 72);
            this.parameter.Name = "parameter";
            this.parameter.Size = new System.Drawing.Size(179, 22);
            this.parameter.TabIndex = 3;
            // 
            // WorkflowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 195);
            this.Controls.Add(this.parameter);
            this.Controls.Add(this.workflowName);
            this.Controls.Add(this.createBtn);
            this.Controls.Add(this.abortBtn);
            this.Controls.Add(this.doneBtn);
            this.Controls.Add(this.execBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.taskList);
            this.Controls.Add(this.workflowList);
            this.Name = "WorkflowForm";
            this.Text = "Test Workflow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox workflowList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox taskList;
        private System.Windows.Forms.Button execBtn;
        private System.Windows.Forms.TextBox workflowName;
        private System.Windows.Forms.Button createBtn;
        private System.Windows.Forms.Button abortBtn;
        private System.Windows.Forms.Button doneBtn;
        private System.Windows.Forms.TextBox parameter;
    }
}

