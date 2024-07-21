var TreeView = function () {

    return {
        //main function to initiate the module
        init: function () {

            var DataSourceTree = function (options) {
                this._data = options.data;
                this._delay = options.delay;
            };

            DataSourceTree.prototype = {

                data: function (options, callback) {
                    var self = this;
                    setTimeout(function () {
                        var data = $.extend(true, [], self._data);
                        callback({ data: data });
                    }, this._delay)
                }
            };

            // INITIALIZING TREE
            var treeDataSource = new DataSourceTree({
                data: [
                    { name: 'T.FAT ERP Workflow', type: 'item', additionalParameters: { id: 'I1' } },
                    { name: 'T.FAT Working Rules (Preferences)', type: 'item', additionalParameters: { id: 'I2' } },
                    { name: 'Company / Accounting Period', type: 'folder', additionalParameters: { id: 'I3' } },
                    { name: 'Branch / Location <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F4' } },
                    { name: 'Database Maintenance <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F5' } },
                    { name: 'Data Health Checkup', type: 'folder', additionalParameters: { id: 'F64' } },
                    { name: 'Export/Import Data (other Software)', type: 'folder', additionalParameters: { id: 'F65' } },
                    { name: 'Export/Import Data with T.FAT entERPrise', type: 'folder', additionalParameters: { id: 'F66' } },
                    { name: 'Users Management <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F67' } },
                    { name: 'Exit T.FAT ERP X4', type: 'item', additionalParameters: { id: 'I7' } }
                ],
                delay: 400
            });

            var treeDataSource2 = new DataSourceTree({
                data: [
                    { name: 'ToolBar <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F7' } },
                    { name: 'View Currency', type: 'item', additionalParameters: { id: 'I8' } },
                    { name: 'T.FAT Business Analysis Centre', type: 'item', additionalParameters: { id: 'I9' } },
                    { name: 'Active Learning Mode', type: 'item', additionalParameters: { id: 'I10' } },
                    { name: 'Show Setup Checklist Window', type: 'item', additionalParameters: { id: 'I11' } },
                    { name: 'Active T.FAT Messenger', type: 'item', additionalParameters: { id: 'I12' } },
                    { name: 'View Error Log', type: 'item', additionalParameters: { id: 'I13' } },

                ],
                delay: 400
            });

            var treeDataSource3 = new DataSourceTree({
                data: [
                    { name: 'Masters Maintenance Centre', type: 'item', additionalParameters: { id: 'I14' } },
                    { name: 'Transactions Rules / Setup <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F8' } },
                    { name: 'Transaction Addon Fields (Extender) <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F9' } },
                    { name: 'Master Addon Fields (Extender) <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F10' } },
                    { name: 'Supporting Masters <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F11' } },
                    { name: 'Account Masters Setup <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F12' } },
                    { name: 'Inventory Masters Setup <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F13' } },
                    { name: 'Manufacturing Related Setup <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F14' } },
                    { name: 'Excise Setup <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F15' } },
                    { name: 'Textile Related Setup <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F16' } },
                    { name: 'Employee Masters <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F17' } },
                    { name: 'Machines / Equipments <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F18' } },
                    { name: 'Fixed Assets Masters <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F19' } },
                    { name: 'Export Management <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F20' } },
                    { name: 'Budgets  Targets <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F21' } },
                    { name: 'Master Addons Updates <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F22' } },
                    { name: 'Quick Masters Update <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F23' } },
                    { name: 'Period Locking Mechanism', type: 'item', additionalParameters: { id: 'I13' } },
                    { name: 'Parameters/Master Updation', type: 'item', additionalParameters: { id: 'I14' } },
                    { name: 'Opening Updations', type: 'item', additionalParameters: { id: 'I15' } },
                    { name: 'Physically Merge Accounts', type: 'item', additionalParameters: { id: 'I16' } },
                    { name: 'Physically Merge Products', type: 'item', additionalParameters: { id: 'I17' } },
                    { name: 'Documents/Formats Designer', type: 'item', additionalParameters: { id: 'I18' } },
                    { name: 'Correspondence/Letters Designer', type: 'item', additionalParameters: { id: 'I19' } },

                ],
                delay: 400
            });

            var treeDataSource4 = new DataSourceTree({
                data: [
                     { name: 'Transaction Maintenance Centre', type: 'item', additionalParameters: { id: 'I20' } },
                     { name: 'Transaction Authorisation Centre', type: 'item', additionalParameters: { id: 'I21' } },
                     { name: 'Daily Cash Collection Transactions', type: 'item', additionalParameters: { id: 'I22' } },
                     { name: 'Employee Timeline Reporting', type: 'item', additionalParameters: { id: 'I23' } },
                     { name: 'Payment Processing System', type: 'item', additionalParameters: { id: 'I24' } },
                     { name: 'Inter-Branch Order Allocations', type: 'item', additionalParameters: { id: 'I25' } },
                     { name: 'Correspondence/Letters Designer', type: 'item', additionalParameters: { id: 'I26' } },
                     { name: ' Transaction Information Update<div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F24' } },
                     { name: 'Quick Cash/Bank Transactions', type: 'item', additionalParameters: { id: 'I27' } },
                     { name: 'Quotations Comparison / Approval', type: 'item', additionalParameters: { id: 'I28' } },
                     { name: 'Distributor  Stock <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F25' } },
                     { name: 'Bar Code Labels Printing', type: 'item', additionalParameters: { id: 'I29' } },
                     { name: 'Memo Voucher Conversion', type: 'item', additionalParameters: { id: 'I30' } },
                     { name: 'Quality Controls <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F26' } },
                     { name: 'Purchase Enquiry Generation', type: 'item', additionalParameters: { id: 'I31' } },
                     { name: 'Purchase Order Generation <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F27' } },
                     { name: 'Delivery Notes Conversion', type: 'item', additionalParameters: { id: 'I32' } },
                     { name: 'Generate Bulk Transactions <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F28' } },
                     { name: 'Generate Manufacturing from Sales', type: 'item', additionalParameters: { id: 'I33' } },
                     { name: 'Stock Reservation System <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F29' } },
                     { name: 'Auto Frequency Vouchers', type: 'item', additionalParameters: { id: 'I34' } },
                     { name: 'Forms Collection Entry', type: 'item', additionalParameters: { id: 'I35' } },
                     { name: 'Document Status Management <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F30' } },
                     { name: 'Call Center Information <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F31' } },
                     { name: 'Excise Forms (Manufacturing) <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F32' } },
                     { name: 'Claimed Detail <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F33' } },
                     { name: 'My own Interface <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F34' } },

                ],
                delay: 400
            });

            var treeDataSource5 = new DataSourceTree({
                data: [
                    { name: 'T.FAT Standard Reports', type: 'item', additionalParameters: { id: 'I36' } },
                    { name: 'T.FAT Report Centre <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F35' } },
                    { name: 'Registers<div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F36' } },
                    { name: 'Ledgers  Accounts Reports<div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F37' } },
                    { name: 'Cash/Bank Reports<div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F38' } },
                    { name: 'Final Accounts<div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F39' } },
                    { name: 'TDS Management <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F40' } },
                    { name: 'VAT/ Service Tax Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F41' } },
                    { name: 'Loan Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F42' } },
                    { name: 'Accounts Receivable / Customers <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F43' } },
                    { name: 'Accounts Payable / Vendors <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F44' } },
                    { name: 'Salesman / Broker Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F45' } },
                    { name: 'Cost-Centre / Projects <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F46' } },
                    { name: 'Budget Control Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F47' } },
                    { name: 'Fixed Assets Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F48' } },
                    { name: 'Stock Status Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F49' } },
                    { name: 'Stock Analysis Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F50' } },
                    { name: 'Production Stock Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F51' } },
                    { name: 'Voucher Chain ', type: 'item', additionalParameters: { id: 'I37' } },
                    { name: 'Inventory Matrix Report <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F52' } },
                    { name: 'Pre-Sales/Purchase Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F53' } },
                    { name: 'Manufacturer Excise Registers <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F54' } },
                    { name: 'Traders Excise Registers <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F55' } },
                    { name: 'Project wise Reports <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F56' } },
                    { name: 'T.FAT Report Writer/Designer', type: 'item', additionalParameters: { id: 'I38' } },
                    { name: 'T.FAT XLS Report Generator <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F57' } },
                    { name: 'T.FAT Query Report Generator <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F58' } },
                ],
                delay: 400
            });

            var treeDataSource6 = new DataSourceTree({
                data: [
                   { name: 'Project Types', type: 'item', additionalParameters: { id: 'I39' } },
                   { name: 'JOB / Projects Setup', type: 'item', additionalParameters: { id: 'I40' } },
                   { name: 'Labour Grades', type: 'item', additionalParameters: { id: 'I41' } },
                   { name: 'Employee Master', type: 'item', additionalParameters: { id: 'I42' } },
                   { name: 'Labour Grade Rates', type: 'item', additionalParameters: { id: 'I43' } },
                   { name: 'Employee Availability Chart', type: 'item', additionalParameters: { id: 'I44' } },
                   { name: 'Labour / ManPower Allocations', type: 'item', additionalParameters: { id: 'I45' } },
                   { name: 'Vendor Capacity / Strength<div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F59' } },
                   { name: 'Stages of Work / Project', type: 'item', additionalParameters: { id: 'I46' } },
                   { name: 'Project Activity (IOW) Type', type: 'item', additionalParameters: { id: 'I47' } },
                   { name: 'Project Activity (Item Of Work-IOW)', type: 'item', additionalParameters: { id: 'I48' } },
                   { name: 'Project / Job Schedule (WBS)', type: 'item', additionalParameters: { id: 'I49' } },
                   { name: 'Project Wise Followup', type: 'item', additionalParameters: { id: 'I50' } },
                   { name: 'Project Resource Requirements', type: 'item', additionalParameters: { id: 'I51' } },
                   { name: 'Project Schedule and Progress', type: 'item', additionalParameters: { id: 'I52' } },
                   { name: 'Project Progress Monitoring', type: 'item', additionalParameters: { id: 'I53' } },
                   { name: 'Project Closing', type: 'item', additionalParameters: { id: 'I54' } },
                ],
                delay: 400
            });

            var treeDataSource7 = new DataSourceTree({
                data: [
                   { name: 'Project Types', type: 'item', additionalParameters: { id: 'I55' } },
                   { name: 'Projects Setup', type: 'item', additionalParameters: { id: 'I56' } },
                   { name: 'Project wise Addons', type: 'item', additionalParameters: { id: 'I57' } },
                   { name: 'Car Parking Slots', type: 'item', additionalParameters: { id: 'I58' } },
                   { name: 'Phases / Stages of Work', type: 'item', additionalParameters: { id: 'I59' } },
                   { name: 'Project Schedules (Stages)', type: 'item', additionalParameters: { id: 'I60' } },
                   { name: 'Types of Flats / Units', type: 'item', additionalParameters: { id: 'I61' } },
                   { name: 'Extra Items per Flat', type: 'item', additionalParameters: { id: 'I62' } },
                   { name: ' Common Amenities', type: 'item', additionalParameters: { id: 'I63' } },
                   { name: ' Amenities Template', type: 'item', additionalParameters: { id: 'I64' } },
                   { name: 'Flat Availability Chart', type: 'item', additionalParameters: { id: 'I65' } },
                   { name: 'Property On Rent', type: 'item', additionalParameters: { id: 'I66' } },
                   { name: 'Flats / Units  Booking', type: 'item', additionalParameters: { id: 'I67' } },
                   { name: 'Transfer Booking to another Unit', type: 'item', additionalParameters: { id: 'I68' } },
                   { name: 'Cancel Booking', type: 'item', additionalParameters: { id: 'I69' } },
                   { name: 'Letters  Correspondence', type: 'item', additionalParameters: { id: 'I70' } },
                   { name: 'Next Due Report', type: 'item', additionalParameters: { id: 'I71' } },
                   { name: 'Flat Installment Schedule', type: 'item', additionalParameters: { id: 'I72' } },
                   { name: 'Booking Status', type: 'item', additionalParameters: { id: 'I73' } },
                   { name: 'Quick Cash/Bank Trans(Projects)', type: 'item', additionalParameters: { id: 'I74' } },
                   { name: 'Land Bank', type: 'item', additionalParameters: { id: 'I75' } },
                   { name: 'Competitors Projects', type: 'item', additionalParameters: { id: 'I76' } },
                   { name: 'Projects Enquiry', type: 'item', additionalParameters: { id: 'I77' } },
                   { name: 'Property Rent Voucher', type: 'item', additionalParameters: { id: 'I78' } },
                   { name: 'Projects Quotation', type: 'item', additionalParameters: { id: 'I79' } },
                ],
                delay: 400
            });

            var treeDataSource8 = new DataSourceTree({
                data: [
                   { name: 'Startup Notifications', type: 'item', additionalParameters: { id: 'I55' } },
                   { name: 'Product History', type: 'item', additionalParameters: { id: 'I56' } },
                   { name: 'Follow Ups', type: 'item', additionalParameters: { id: 'I57' } },
                   { name: 'FollowUp Status Report', type: 'item', additionalParameters: { id: 'I58' } },
                   { name: 'Charts  Graphs', type: 'item', additionalParameters: { id: 'I59' } },
                   { name: 'Expense Comparative Summary', type: 'item', additionalParameters: { id: 'I60' } },
                   { name: 'Monthly Distributor Revenue', type: 'item', additionalParameters: { id: 'I61' } },
                   { name: 'Entries made on Holidays', type: 'item', additionalParameters: { id: 'I62' } },
                   { name: 'Internal Audit', type: 'item', additionalParameters: { id: 'I63' } },
                   { name: 'Report Mailer', type: 'item', additionalParameters: { id: 'I64' } },
                   { name: 'Search Information', type: 'item', additionalParameters: { id: 'I65' } },
                   { name: 'Communication Log', type: 'item', additionalParameters: { id: 'I66' } },

                ],
                delay: 400
            });

            var treeDataSource9 = new DataSourceTree({
                data: [
                   { name: 'Calculator', type: 'item', additionalParameters: { id: 'I67' } },
                   { name: 'Email Interface', type: 'item', additionalParameters: { id: 'I68' } },
                   { name: 'SMS Gateway Interface', type: 'item', additionalParameters: { id: 'I69' } },
                   { name: 'QR Code Generator', type: 'item', additionalParameters: { id: 'I70' } },
                   { name: 'T.FAT NotePad', type: 'item', additionalParameters: { id: 'I71' } },
                   { name: ' T.FAT MyPlanner', type: 'item', additionalParameters: { id: 'I72' } },
                   { name: 'ToolBar Designer', type: 'item', additionalParameters: { id: 'I73' } },
                   { name: 'Image Viewer', type: 'item', additionalParameters: { id: 'I74' } },
                   { name: 'T.FAT Data Manager', type: 'item', additionalParameters: { id: 'I75' } },
                   { name: ' T.FAT Messenger', type: 'item', additionalParameters: { id: 'I76' } },
                   { name: 'Task Management <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F60' } },
                   { name: ' Colour Schemes <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F61' } },
                   { name: 'Dialer  Information Interface', type: 'item', additionalParameters: { id: 'I77' } },
                   { name: 'Contct Management', type: 'item', additionalParameters: { id: 'I78' } },
                   { name: 'Remove Unused Masters <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F62' } },
                   { name: 'Audit Trails  User Activities', type: 'item', additionalParameters: { id: 'I79' } },
                   { name: 'Missing Vouchers', type: 'item', additionalParameters: { id: 'I80' } },

                ],
                delay: 400
            });

            var treeDataSource10 = new DataSourceTree({
                data: [
                   { name: 'About T.FAT', type: 'item', additionalParameters: { id: 'I81' } },
                   { name: 'Users Guide', type: 'item', additionalParameters: { id: 'I82' } },
                   { name: 'Learning Centre Tutorials', type: 'item', additionalParameters: { id: 'I83' } },
                   { name: 'TFAT Download and Help/Support Centre', type: 'item', additionalParameters: { id: 'I84' } },
                   { name: 'Check for Software Updates', type: 'item', additionalParameters: { id: 'I85' } },
                   { name: 'Tip of the Day', type: 'item', additionalParameters: { id: 'I86' } },
                   { name: 'Report an Error / Bug', type: 'item', additionalParameters: { id: 'I87' } },
                   { name: 'Send Suggestions', type: 'item', additionalParameters: { id: 'I88' } },
                   { name: 'Request Software Customisation', type: 'item', additionalParameters: { id: 'I89' } },
                   { name: 'Request for Web Solutions', type: 'item', additionalParameters: { id: 'I90' } },
                   { name: 'Software Registration', type: 'item', additionalParameters: { id: 'I91' } },
                   { name: 'Customer Feedback form', type: 'item', additionalParameters: { id: 'I92' } },
                   { name: 'View Software License (EULA)', type: 'item', additionalParameters: { id: 'I93' } },
                   { name: 'Remote Support (Team-Viewer)', type: 'item', additionalParameters: { id: 'I94' } },
                   { name: 'Remote Support (Ammyy)', type: 'item', additionalParameters: { id: 'I95' } },
                   { name: 'Remote Support (ShowMyPC)', type: 'item', additionalParameters: { id: 'I96' } },
                   { name: 'Hotkeys', type: 'item', additionalParameters: { id: 'I97' } },
                   { name: 'Quick Menu <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F63' } },

                ],
                delay: 400
            });

            $('#File').tree({
                dataSource: treeDataSource,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });


            $('#View').tree({
                dataSource: treeDataSource2,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#Setup').tree({
                dataSource: treeDataSource3,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#Transaction').tree({
                dataSource: treeDataSource4,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#Reports').tree({
                dataSource: treeDataSource5,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#ProjectsMgmt').tree({
                dataSource: treeDataSource6,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#ProjectsCRM').tree({
                dataSource: treeDataSource7,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#MIS').tree({
                dataSource: treeDataSource8,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#HouseKeepingUtilities').tree({
                dataSource: treeDataSource9,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });

            $('#Help').tree({
                dataSource: treeDataSource10,
                loadingHTML: '<img src="images/input-spinner.gif"/>',
            });


        }

    };

}();



var TreeView1 = function () {
    return {
        //main function to initiate the module
        init: function () {

            var DataSourceTree = function (options) {
                this._data = options.data;
                this._delay = options.delay;
            };

            DataSourceTree.prototype = {

                data: function (options, callback) {
                    var self = this;
                    setTimeout(function () {
                        var data = $.extend(true, [], self._data);
                        callback({ data: data });
                    }, this._delay)
                }
            };

            // INITIALIZING TREE
            var treeDataSource = new DataSourceTree({
                data: [
                    { name: 'T.FAT ERP Workflow', type: 'item', additionalParameters: { id: 'I1' } },
                    { name: 'T.FAT Working Rules (Preferences)', type: 'item', additionalParameters: { id: 'I2' } },
                    { name: 'Company / Accounting Period', type: 'folder', additionalParameters: { id: 'I3' } },
                    { name: 'Branch / Location <div class="tree-actions"></div>', type: 'folder', additionalParameters: { id: 'F4' } },
                ],
                delay: 400
            });

        }

    };

}();