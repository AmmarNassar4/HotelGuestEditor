namespace HotelGuestEditor
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblResNub = new Label();
            txtResNub = new TextBox();
            btnLoadGuests = new Button();
            btnScanPassport = new Button();
            btnPassportStandby = new Button();
            lblGuests = new Label();
            cmbGuests = new ComboBox();
            btnLoadSelectedGuest = new Button();
            lblGstCod = new Label();
            txtGstCod = new TextBox();
            lblSrlNub = new Label();
            txtSrlNub = new TextBox();
            lblSubSrl = new Label();
            txtSubSrl = new TextBox();
            lblTitle = new Label();
            txtTitle = new TextBox();
            lblFirstName = new Label();
            txtFirstName = new TextBox();
            lblMiddleName = new Label();
            txtMiddleName = new TextBox();
            lblLastName = new Label();
            txtLastName = new TextBox();
            lblAddress = new Label();
            txtAddress = new TextBox();
            lblCity = new Label();
            txtCity = new TextBox();
            lblState = new Label();
            txtState = new TextBox();
            lblCountry = new Label();
            txtCountry = new TextBox();
            lblZip = new Label();
            txtZip = new TextBox();
            lblPhone = new Label();
            txtPhone = new TextBox();
            lblMobile = new Label();
            txtMobile = new TextBox();
            lblNationality = new Label();
            txtNationality = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblGender = new Label();
            cmbGender = new ComboBox();
            lblGstn = new Label();
            txtGstn = new TextBox();
            lblDocumentType = new Label();
            txtDocumentType = new TextBox();
            lblPassportNumber = new Label();
            txtPassportNumber = new TextBox();
            lblPassportIssuePlace = new Label();
            txtPassportIssuePlace = new TextBox();
            lblIdType = new Label();
            txtIdType = new TextBox();
            lblIdNumber = new Label();
            txtIdNumber = new TextBox();
            btnSave = new Button();
            button3 = new Button();
            button2 = new Button();
            lblArrivalSearchDate = new Label();
            dtArrivalSearchDate = new DateTimePicker();
            btnSearchReservations = new Button();
            lblReservationNameFilter = new Label();
            txtReservationNameFilter = new TextBox();
            dgvReservations = new DataGridView();
            dtExpiryDate = new DateTimePicker();
            chkExpiryDate = new CheckBox();
            lblExpiryDate = new Label();
            dtIssueDate = new DateTimePicker();
            chkIssueDate = new CheckBox();
            lblIssueDate = new Label();
            dtBirthDate = new DateTimePicker();
            chkBirthDate = new CheckBox();
            lblBirthDate = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvReservations).BeginInit();
            SuspendLayout();
            // 
            // lblResNub
            // 
            lblResNub.AutoSize = true;
            lblResNub.Location = new Point(30, 314);
            lblResNub.Name = "lblResNub";
            lblResNub.Size = new Size(115, 15);
            lblResNub.TabIndex = 0;
            lblResNub.Text = "Reservation Number";
            lblResNub.Visible = false;
            // 
            // txtResNub
            // 
            txtResNub.Location = new Point(230, 310);
            txtResNub.Name = "txtResNub";
            txtResNub.ReadOnly = true;
            txtResNub.Size = new Size(260, 23);
            txtResNub.TabIndex = 1;
            txtResNub.Visible = false;
            // 
            // btnLoadGuests
            // 
            btnLoadGuests.Location = new Point(510, 308);
            btnLoadGuests.Name = "btnLoadGuests";
            btnLoadGuests.Size = new Size(120, 28);
            btnLoadGuests.TabIndex = 2;
            btnLoadGuests.Text = "Load Guests";
            btnLoadGuests.UseVisualStyleBackColor = true;
            btnLoadGuests.Visible = false;
            btnLoadGuests.Click += btnLoadGuests_Click;
            // 
            // btnScanPassport
            // 
            btnScanPassport.Location = new Point(637, 591);
            btnScanPassport.Name = "btnScanPassport";
            btnScanPassport.Size = new Size(95, 28);
            btnScanPassport.TabIndex = 3;
            btnScanPassport.Text = "Scan Passport";
            btnScanPassport.UseVisualStyleBackColor = true;
            btnScanPassport.Visible = false;
            btnScanPassport.Click += btnScanPassport_Click;
            // 
            // btnPassportStandby
            // 
            btnPassportStandby.Location = new Point(738, 591);
            btnPassportStandby.Name = "btnPassportStandby";
            btnPassportStandby.Size = new Size(90, 28);
            btnPassportStandby.TabIndex = 104;
            btnPassportStandby.Text = "Standby";
            btnPassportStandby.UseVisualStyleBackColor = true;
            btnPassportStandby.Click += btnPassportStandby_Click;
            // 
            // lblGuests
            // 
            lblGuests.AutoSize = true;
            lblGuests.Location = new Point(30, 354);
            lblGuests.Name = "lblGuests";
            lblGuests.Size = new Size(114, 15);
            lblGuests.TabIndex = 4;
            lblGuests.Text = "Guest In Reservation";
            lblGuests.Visible = false;
            // 
            // cmbGuests
            // 
            cmbGuests.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGuests.FormattingEnabled = true;
            cmbGuests.Location = new Point(230, 350);
            cmbGuests.Name = "cmbGuests";
            cmbGuests.Size = new Size(400, 23);
            cmbGuests.TabIndex = 5;
            cmbGuests.Visible = false;
            // 
            // btnLoadSelectedGuest
            // 
            btnLoadSelectedGuest.Location = new Point(660, 350);
            btnLoadSelectedGuest.Name = "btnLoadSelectedGuest";
            btnLoadSelectedGuest.Size = new Size(150, 28);
            btnLoadSelectedGuest.TabIndex = 6;
            btnLoadSelectedGuest.Text = "Load Selected Guest";
            btnLoadSelectedGuest.UseVisualStyleBackColor = true;
            btnLoadSelectedGuest.Visible = false;
            btnLoadSelectedGuest.Click += btnLoadSelectedGuest_Click;
            // 
            // lblGstCod
            // 
            lblGstCod.AutoSize = true;
            lblGstCod.Location = new Point(30, 399);
            lblGstCod.Name = "lblGstCod";
            lblGstCod.Size = new Size(68, 15);
            lblGstCod.TabIndex = 7;
            lblGstCod.Text = "Guest Code";
            lblGstCod.Visible = false;
            // 
            // txtGstCod
            // 
            txtGstCod.Location = new Point(230, 395);
            txtGstCod.Name = "txtGstCod";
            txtGstCod.ReadOnly = true;
            txtGstCod.Size = new Size(260, 23);
            txtGstCod.TabIndex = 8;
            txtGstCod.Visible = false;
            txtGstCod.TextChanged += txtGstCod_TextChanged;
            // 
            // lblSrlNub
            // 
            lblSrlNub.AutoSize = true;
            lblSrlNub.Location = new Point(30, 431);
            lblSrlNub.Name = "lblSrlNub";
            lblSrlNub.Size = new Size(99, 15);
            lblSrlNub.TabIndex = 9;
            lblSrlNub.Text = "Reservation Serial";
            lblSrlNub.Visible = false;
            // 
            // txtSrlNub
            // 
            txtSrlNub.Location = new Point(230, 427);
            txtSrlNub.Name = "txtSrlNub";
            txtSrlNub.ReadOnly = true;
            txtSrlNub.Size = new Size(260, 23);
            txtSrlNub.TabIndex = 10;
            txtSrlNub.Visible = false;
            // 
            // lblSubSrl
            // 
            lblSubSrl.AutoSize = true;
            lblSubSrl.Location = new Point(30, 463);
            lblSubSrl.Name = "lblSubSrl";
            lblSubSrl.Size = new Size(68, 15);
            lblSubSrl.TabIndex = 11;
            lblSubSrl.Text = "Guest Serial";
            lblSubSrl.Visible = false;
            // 
            // txtSubSrl
            // 
            txtSubSrl.Location = new Point(230, 459);
            txtSubSrl.Name = "txtSubSrl";
            txtSubSrl.ReadOnly = true;
            txtSubSrl.Size = new Size(260, 23);
            txtSubSrl.TabIndex = 12;
            txtSubSrl.Visible = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(30, 495);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(30, 15);
            lblTitle.TabIndex = 13;
            lblTitle.Text = "Title";
            lblTitle.Visible = false;
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(230, 491);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(260, 23);
            txtTitle.TabIndex = 14;
            txtTitle.Visible = false;
            // 
            // lblFirstName
            // 
            lblFirstName.AutoSize = true;
            lblFirstName.Location = new Point(30, 527);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(64, 15);
            lblFirstName.TabIndex = 15;
            lblFirstName.Text = "First Name";
            lblFirstName.Visible = false;
            // 
            // txtFirstName
            // 
            txtFirstName.Location = new Point(230, 523);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(260, 23);
            txtFirstName.TabIndex = 16;
            txtFirstName.Visible = false;
            // 
            // lblMiddleName
            // 
            lblMiddleName.AutoSize = true;
            lblMiddleName.Location = new Point(30, 559);
            lblMiddleName.Name = "lblMiddleName";
            lblMiddleName.Size = new Size(79, 15);
            lblMiddleName.TabIndex = 17;
            lblMiddleName.Text = "Middle Name";
            lblMiddleName.Visible = false;
            // 
            // txtMiddleName
            // 
            txtMiddleName.Location = new Point(230, 555);
            txtMiddleName.Name = "txtMiddleName";
            txtMiddleName.Size = new Size(260, 23);
            txtMiddleName.TabIndex = 18;
            txtMiddleName.Visible = false;
            // 
            // lblLastName
            // 
            lblLastName.AutoSize = true;
            lblLastName.Location = new Point(30, 591);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(63, 15);
            lblLastName.TabIndex = 19;
            lblLastName.Text = "Last Name";
            lblLastName.Visible = false;
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(230, 587);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(260, 23);
            txtLastName.TabIndex = 20;
            txtLastName.Visible = false;
            // 
            // lblAddress
            // 
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(30, 623);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(49, 15);
            lblAddress.TabIndex = 21;
            lblAddress.Text = "Address";
            lblAddress.Visible = false;
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(230, 619);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(260, 23);
            txtAddress.TabIndex = 22;
            txtAddress.Visible = false;
            // 
            // lblCity
            // 
            lblCity.AutoSize = true;
            lblCity.Location = new Point(30, 655);
            lblCity.Name = "lblCity";
            lblCity.Size = new Size(28, 15);
            lblCity.TabIndex = 23;
            lblCity.Text = "City";
            lblCity.Visible = false;
            // 
            // txtCity
            // 
            txtCity.Location = new Point(230, 651);
            txtCity.Name = "txtCity";
            txtCity.Size = new Size(260, 23);
            txtCity.TabIndex = 24;
            txtCity.Visible = false;
            // 
            // lblState
            // 
            lblState.AutoSize = true;
            lblState.Location = new Point(30, 687);
            lblState.Name = "lblState";
            lblState.Size = new Size(33, 15);
            lblState.TabIndex = 25;
            lblState.Text = "State";
            lblState.Visible = false;
            // 
            // txtState
            // 
            txtState.Location = new Point(230, 683);
            txtState.Name = "txtState";
            txtState.Size = new Size(260, 23);
            txtState.TabIndex = 26;
            txtState.Visible = false;
            // 
            // lblCountry
            // 
            lblCountry.AutoSize = true;
            lblCountry.Location = new Point(30, 719);
            lblCountry.Name = "lblCountry";
            lblCountry.Size = new Size(50, 15);
            lblCountry.TabIndex = 27;
            lblCountry.Text = "Country";
            lblCountry.Visible = false;
            // 
            // txtCountry
            // 
            txtCountry.Location = new Point(230, 715);
            txtCountry.Name = "txtCountry";
            txtCountry.Size = new Size(260, 23);
            txtCountry.TabIndex = 28;
            txtCountry.Visible = false;
            // 
            // lblZip
            // 
            lblZip.AutoSize = true;
            lblZip.Location = new Point(30, 751);
            lblZip.Name = "lblZip";
            lblZip.Size = new Size(24, 15);
            lblZip.TabIndex = 29;
            lblZip.Text = "Zip";
            lblZip.Visible = false;
            // 
            // txtZip
            // 
            txtZip.Location = new Point(230, 747);
            txtZip.Name = "txtZip";
            txtZip.Size = new Size(260, 23);
            txtZip.TabIndex = 30;
            txtZip.Visible = false;
            // 
            // lblPhone
            // 
            lblPhone.AutoSize = true;
            lblPhone.Location = new Point(30, 783);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(41, 15);
            lblPhone.TabIndex = 31;
            lblPhone.Text = "Phone";
            lblPhone.Visible = false;
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(230, 779);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(260, 23);
            txtPhone.TabIndex = 32;
            txtPhone.Visible = false;
            // 
            // lblMobile
            // 
            lblMobile.AutoSize = true;
            lblMobile.Location = new Point(30, 815);
            lblMobile.Name = "lblMobile";
            lblMobile.Size = new Size(44, 15);
            lblMobile.TabIndex = 33;
            lblMobile.Text = "Mobile";
            lblMobile.Visible = false;
            // 
            // txtMobile
            // 
            txtMobile.Location = new Point(230, 811);
            txtMobile.Name = "txtMobile";
            txtMobile.Size = new Size(260, 23);
            txtMobile.TabIndex = 34;
            txtMobile.Visible = false;
            // 
            // lblNationality
            // 
            lblNationality.AutoSize = true;
            lblNationality.Location = new Point(30, 847);
            lblNationality.Name = "lblNationality";
            lblNationality.Size = new Size(65, 15);
            lblNationality.TabIndex = 35;
            lblNationality.Text = "Nationality";
            lblNationality.Visible = false;
            // 
            // txtNationality
            // 
            txtNationality.Location = new Point(230, 843);
            txtNationality.Name = "txtNationality";
            txtNationality.Size = new Size(260, 23);
            txtNationality.TabIndex = 36;
            txtNationality.Visible = false;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(30, 879);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(36, 15);
            lblEmail.TabIndex = 37;
            lblEmail.Text = "Email";
            lblEmail.Visible = false;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(230, 875);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(260, 23);
            txtEmail.TabIndex = 38;
            txtEmail.Visible = false;
            // 
            // lblGender
            // 
            lblGender.AutoSize = true;
            lblGender.Location = new Point(30, 911);
            lblGender.Name = "lblGender";
            lblGender.Size = new Size(45, 15);
            lblGender.TabIndex = 39;
            lblGender.Text = "Gender";
            lblGender.Visible = false;
            // 
            // cmbGender
            // 
            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.FormattingEnabled = true;
            cmbGender.Location = new Point(230, 907);
            cmbGender.Name = "cmbGender";
            cmbGender.Size = new Size(260, 23);
            cmbGender.TabIndex = 40;
            cmbGender.Visible = false;
            // 
            // lblGstn
            // 
            lblGstn.AutoSize = true;
            lblGstn.Location = new Point(30, 943);
            lblGstn.Name = "lblGstn";
            lblGstn.Size = new Size(37, 15);
            lblGstn.TabIndex = 41;
            lblGstn.Text = "GSTN";
            lblGstn.Visible = false;
            // 
            // txtGstn
            // 
            txtGstn.Location = new Point(230, 939);
            txtGstn.Name = "txtGstn";
            txtGstn.Size = new Size(260, 23);
            txtGstn.TabIndex = 42;
            txtGstn.Visible = false;
            // 
            // lblDocumentType
            // 
            lblDocumentType.AutoSize = true;
            lblDocumentType.Location = new Point(30, 975);
            lblDocumentType.Name = "lblDocumentType";
            lblDocumentType.Size = new Size(91, 15);
            lblDocumentType.TabIndex = 43;
            lblDocumentType.Text = "Document Type";
            lblDocumentType.Visible = false;
            // 
            // txtDocumentType
            // 
            txtDocumentType.Location = new Point(230, 971);
            txtDocumentType.Name = "txtDocumentType";
            txtDocumentType.Size = new Size(260, 23);
            txtDocumentType.TabIndex = 44;
            txtDocumentType.Visible = false;
            // 
            // lblPassportNumber
            // 
            lblPassportNumber.AutoSize = true;
            lblPassportNumber.Location = new Point(30, 1007);
            lblPassportNumber.Name = "lblPassportNumber";
            lblPassportNumber.Size = new Size(99, 15);
            lblPassportNumber.TabIndex = 45;
            lblPassportNumber.Text = "Passport Number";
            lblPassportNumber.Visible = false;
            // 
            // txtPassportNumber
            // 
            txtPassportNumber.Location = new Point(230, 1003);
            txtPassportNumber.Name = "txtPassportNumber";
            txtPassportNumber.Size = new Size(260, 23);
            txtPassportNumber.TabIndex = 46;
            txtPassportNumber.Visible = false;
            // 
            // lblPassportIssuePlace
            // 
            lblPassportIssuePlace.AutoSize = true;
            lblPassportIssuePlace.Location = new Point(30, 1039);
            lblPassportIssuePlace.Name = "lblPassportIssuePlace";
            lblPassportIssuePlace.Size = new Size(112, 15);
            lblPassportIssuePlace.TabIndex = 47;
            lblPassportIssuePlace.Text = "Passport Issue Place";
            lblPassportIssuePlace.Visible = false;
            // 
            // txtPassportIssuePlace
            // 
            txtPassportIssuePlace.Location = new Point(230, 1035);
            txtPassportIssuePlace.Name = "txtPassportIssuePlace";
            txtPassportIssuePlace.Size = new Size(260, 23);
            txtPassportIssuePlace.TabIndex = 48;
            txtPassportIssuePlace.Visible = false;
            // 
            // lblIdType
            // 
            lblIdType.AutoSize = true;
            lblIdType.Location = new Point(30, 1071);
            lblIdType.Name = "lblIdType";
            lblIdType.Size = new Size(89, 15);
            lblIdType.TabIndex = 49;
            lblIdType.Text = "Identity Type ID";
            lblIdType.Visible = false;
            // 
            // txtIdType
            // 
            txtIdType.Location = new Point(230, 1067);
            txtIdType.MaxLength = 2;
            txtIdType.Name = "txtIdType";
            txtIdType.Size = new Size(260, 23);
            txtIdType.TabIndex = 50;
            txtIdType.Visible = false;
            // 
            // lblIdNumber
            // 
            lblIdNumber.AutoSize = true;
            lblIdNumber.Location = new Point(30, 1103);
            lblIdNumber.Name = "lblIdNumber";
            lblIdNumber.Size = new Size(94, 15);
            lblIdNumber.TabIndex = 51;
            lblIdNumber.Text = "Identity Number";
            lblIdNumber.Visible = false;
            // 
            // txtIdNumber
            // 
            txtIdNumber.Location = new Point(230, 1099);
            txtIdNumber.Name = "txtIdNumber";
            txtIdNumber.Size = new Size(260, 23);
            txtIdNumber.TabIndex = 52;
            txtIdNumber.Visible = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(636, 543);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(192, 35);
            btnSave.TabIndex = 62;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Visible = false;
            btnSave.Click += btnSave_Click;
            // 
            // button3
            // 
            button3.Location = new Point(660, 315);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 63;
            button3.Text = "Start";
            button3.UseVisualStyleBackColor = true;
            button3.Visible = false;
            // 
            // button2
            // 
            button2.Enabled = false;
            button2.Location = new Point(741, 315);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 63;
            button2.Text = "Stop";
            button2.UseVisualStyleBackColor = true;
            button2.Visible = false;
            // 
            // lblArrivalSearchDate
            // 
            lblArrivalSearchDate.AutoSize = true;
            lblArrivalSearchDate.Location = new Point(30, 24);
            lblArrivalSearchDate.Name = "lblArrivalSearchDate";
            lblArrivalSearchDate.Size = new Size(68, 15);
            lblArrivalSearchDate.TabIndex = 100;
            lblArrivalSearchDate.Text = "Arrival Date";
            // 
            // dtArrivalSearchDate
            // 
            dtArrivalSearchDate.Format = DateTimePickerFormat.Short;
            dtArrivalSearchDate.Location = new Point(130, 20);
            dtArrivalSearchDate.Name = "dtArrivalSearchDate";
            dtArrivalSearchDate.Size = new Size(150, 23);
            dtArrivalSearchDate.TabIndex = 101;
            // 
            // btnSearchReservations
            // 
            btnSearchReservations.Location = new Point(300, 18);
            btnSearchReservations.Name = "btnSearchReservations";
            btnSearchReservations.Size = new Size(120, 28);
            btnSearchReservations.TabIndex = 102;
            btnSearchReservations.Text = "Search";
            btnSearchReservations.UseVisualStyleBackColor = true;
            btnSearchReservations.Click += btnSearchReservations_Click;
            // 
            // lblReservationNameFilter
            // 
            lblReservationNameFilter.AutoSize = true;
            lblReservationNameFilter.Location = new Point(440, 24);
            lblReservationNameFilter.Name = "lblReservationNameFilter";
            lblReservationNameFilter.Size = new Size(74, 15);
            lblReservationNameFilter.TabIndex = 114;
            lblReservationNameFilter.Text = "Name Search";
            // 
            // txtReservationNameFilter
            // 
            txtReservationNameFilter.Location = new Point(530, 20);
            txtReservationNameFilter.Name = "txtReservationNameFilter";
            txtReservationNameFilter.PlaceholderText = "Type guest name...";
            txtReservationNameFilter.Size = new Size(320, 23);
            txtReservationNameFilter.TabIndex = 115;
            txtReservationNameFilter.TextChanged += txtReservationNameFilter_TextChanged;
            // 
            // dgvReservations
            // 
            dgvReservations.AllowUserToAddRows = false;
            dgvReservations.AllowUserToDeleteRows = false;
            dgvReservations.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvReservations.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReservations.BackgroundColor = SystemColors.Window;
            dgvReservations.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReservations.Location = new Point(30, 60);
            dgvReservations.MultiSelect = false;
            dgvReservations.Name = "dgvReservations";
            dgvReservations.ReadOnly = true;
            dgvReservations.RowHeadersVisible = false;
            dgvReservations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReservations.Size = new Size(735, 230);
            dgvReservations.TabIndex = 103;
            dgvReservations.CellDoubleClick += dgvReservations_CellDoubleClick;
            dgvReservations.ColumnHeaderMouseClick += dgvReservations_ColumnHeaderMouseClick;
            dgvReservations.KeyDown += dgvReservations_KeyDown;
            // 
            // dtExpiryDate
            // 
            dtExpiryDate.Format = DateTimePickerFormat.Short;
            dtExpiryDate.Location = new Point(636, 491);
            dtExpiryDate.Name = "dtExpiryDate";
            dtExpiryDate.Size = new Size(235, 23);
            dtExpiryDate.TabIndex = 113;
            dtExpiryDate.Visible = false;
            // 
            // chkExpiryDate
            // 
            chkExpiryDate.Location = new Point(611, 491);
            chkExpiryDate.Name = "chkExpiryDate";
            chkExpiryDate.Size = new Size(18, 24);
            chkExpiryDate.TabIndex = 112;
            chkExpiryDate.Visible = false;
            // 
            // lblExpiryDate
            // 
            lblExpiryDate.AutoSize = true;
            lblExpiryDate.Location = new Point(534, 499);
            lblExpiryDate.Name = "lblExpiryDate";
            lblExpiryDate.Size = new Size(65, 15);
            lblExpiryDate.TabIndex = 111;
            lblExpiryDate.Text = "Expiry Date";
            lblExpiryDate.Visible = false;
            // 
            // dtIssueDate
            // 
            dtIssueDate.Format = DateTimePickerFormat.Short;
            dtIssueDate.Location = new Point(636, 459);
            dtIssueDate.Name = "dtIssueDate";
            dtIssueDate.Size = new Size(235, 23);
            dtIssueDate.TabIndex = 110;
            dtIssueDate.Visible = false;
            // 
            // chkIssueDate
            // 
            chkIssueDate.Location = new Point(611, 459);
            chkIssueDate.Name = "chkIssueDate";
            chkIssueDate.Size = new Size(18, 24);
            chkIssueDate.TabIndex = 109;
            chkIssueDate.Visible = false;
            // 
            // lblIssueDate
            // 
            lblIssueDate.AutoSize = true;
            lblIssueDate.Location = new Point(534, 467);
            lblIssueDate.Name = "lblIssueDate";
            lblIssueDate.Size = new Size(60, 15);
            lblIssueDate.TabIndex = 108;
            lblIssueDate.Text = "Issue Date";
            lblIssueDate.Visible = false;
            // 
            // dtBirthDate
            // 
            dtBirthDate.Format = DateTimePickerFormat.Short;
            dtBirthDate.Location = new Point(636, 427);
            dtBirthDate.Name = "dtBirthDate";
            dtBirthDate.Size = new Size(235, 23);
            dtBirthDate.TabIndex = 107;
            dtBirthDate.Visible = false;
            // 
            // chkBirthDate
            // 
            chkBirthDate.Location = new Point(611, 427);
            chkBirthDate.Name = "chkBirthDate";
            chkBirthDate.Size = new Size(18, 24);
            chkBirthDate.TabIndex = 106;
            chkBirthDate.Visible = false;
            // 
            // lblBirthDate
            // 
            lblBirthDate.AutoSize = true;
            lblBirthDate.Location = new Point(534, 435);
            lblBirthDate.Name = "lblBirthDate";
            lblBirthDate.Size = new Size(59, 15);
            lblBirthDate.TabIndex = 105;
            lblBirthDate.Text = "Birth Date";
            lblBirthDate.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(900, 1040);
            Controls.Add(dtExpiryDate);
            Controls.Add(chkExpiryDate);
            Controls.Add(lblExpiryDate);
            Controls.Add(dtIssueDate);
            Controls.Add(chkIssueDate);
            Controls.Add(lblIssueDate);
            Controls.Add(dtBirthDate);
            Controls.Add(chkBirthDate);
            Controls.Add(lblBirthDate);
            Controls.Add(txtReservationNameFilter);
            Controls.Add(lblReservationNameFilter);
            Controls.Add(dgvReservations);
            Controls.Add(btnSearchReservations);
            Controls.Add(dtArrivalSearchDate);
            Controls.Add(lblArrivalSearchDate);
            Controls.Add(button2);
            Controls.Add(button3);
            Controls.Add(btnSave);
            Controls.Add(txtIdNumber);
            Controls.Add(lblIdNumber);
            Controls.Add(txtIdType);
            Controls.Add(lblIdType);
            Controls.Add(txtPassportIssuePlace);
            Controls.Add(lblPassportIssuePlace);
            Controls.Add(txtPassportNumber);
            Controls.Add(lblPassportNumber);
            Controls.Add(txtDocumentType);
            Controls.Add(lblDocumentType);
            Controls.Add(txtGstn);
            Controls.Add(lblGstn);
            Controls.Add(cmbGender);
            Controls.Add(lblGender);
            Controls.Add(txtEmail);
            Controls.Add(lblEmail);
            Controls.Add(txtNationality);
            Controls.Add(lblNationality);
            Controls.Add(txtMobile);
            Controls.Add(lblMobile);
            Controls.Add(txtPhone);
            Controls.Add(lblPhone);
            Controls.Add(txtZip);
            Controls.Add(lblZip);
            Controls.Add(txtCountry);
            Controls.Add(lblCountry);
            Controls.Add(txtState);
            Controls.Add(lblState);
            Controls.Add(txtCity);
            Controls.Add(lblCity);
            Controls.Add(txtAddress);
            Controls.Add(lblAddress);
            Controls.Add(txtLastName);
            Controls.Add(lblLastName);
            Controls.Add(txtMiddleName);
            Controls.Add(lblMiddleName);
            Controls.Add(txtFirstName);
            Controls.Add(lblFirstName);
            Controls.Add(txtTitle);
            Controls.Add(lblTitle);
            Controls.Add(txtSubSrl);
            Controls.Add(lblSubSrl);
            Controls.Add(txtSrlNub);
            Controls.Add(lblSrlNub);
            Controls.Add(txtGstCod);
            Controls.Add(lblGstCod);
            Controls.Add(btnLoadSelectedGuest);
            Controls.Add(cmbGuests);
            Controls.Add(lblGuests);
            Controls.Add(btnPassportStandby);
            Controls.Add(btnScanPassport);
            Controls.Add(btnLoadGuests);
            Controls.Add(txtResNub);
            Controls.Add(lblResNub);
            Name = "Form1";
            Text = "Reservation Guest Editor";
            ((System.ComponentModel.ISupportInitialize)dgvReservations).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblResNub;
        private System.Windows.Forms.TextBox txtResNub;
        private System.Windows.Forms.Button btnLoadGuests;
        private System.Windows.Forms.Button btnScanPassport;
        private System.Windows.Forms.Button btnPassportStandby;
        private System.Windows.Forms.Label lblGuests;
        private System.Windows.Forms.ComboBox cmbGuests;
        private System.Windows.Forms.Button btnLoadSelectedGuest;
        private System.Windows.Forms.Label lblGstCod;
        private System.Windows.Forms.TextBox txtGstCod;
        private System.Windows.Forms.Label lblSrlNub;
        private System.Windows.Forms.TextBox txtSrlNub;
        private System.Windows.Forms.Label lblSubSrl;
        private System.Windows.Forms.TextBox txtSubSrl;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblFirstName;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.Label lblMiddleName;
        private System.Windows.Forms.TextBox txtMiddleName;
        private System.Windows.Forms.Label lblLastName;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.TextBox txtCity;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.TextBox txtState;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.TextBox txtCountry;
        private System.Windows.Forms.Label lblZip;
        private System.Windows.Forms.TextBox txtZip;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblMobile;
        private System.Windows.Forms.TextBox txtMobile;
        private System.Windows.Forms.Label lblNationality;
        private System.Windows.Forms.TextBox txtNationality;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblGender;
        private System.Windows.Forms.ComboBox cmbGender;
        private System.Windows.Forms.Label lblGstn;
        private System.Windows.Forms.TextBox txtGstn;
        private System.Windows.Forms.Label lblDocumentType;
        private System.Windows.Forms.TextBox txtDocumentType;
        private System.Windows.Forms.Label lblPassportNumber;
        private System.Windows.Forms.TextBox txtPassportNumber;
        private System.Windows.Forms.Label lblPassportIssuePlace;
        private System.Windows.Forms.TextBox txtPassportIssuePlace;
        private System.Windows.Forms.Label lblIdType;
        private System.Windows.Forms.TextBox txtIdType;
        private System.Windows.Forms.Label lblIdNumber;
        private System.Windows.Forms.TextBox txtIdNumber;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblArrivalSearchDate;
        private System.Windows.Forms.DateTimePicker dtArrivalSearchDate;
        private System.Windows.Forms.Button btnSearchReservations;
        private System.Windows.Forms.Label lblReservationNameFilter;
        private System.Windows.Forms.TextBox txtReservationNameFilter;
        private System.Windows.Forms.DataGridView dgvReservations;
        private Button button3;
        private Button button2;
        private DateTimePicker dtExpiryDate;
        private CheckBox chkExpiryDate;
        private Label lblExpiryDate;
        private DateTimePicker dtIssueDate;
        private CheckBox chkIssueDate;
        private Label lblIssueDate;
        private DateTimePicker dtBirthDate;
        private CheckBox chkBirthDate;
        private Label lblBirthDate;
    }
}