﻿/*
    Copyright 2007, Joe Davidson <joedavidson@gmail.com>

    This file is part of FFTPatcher.

    LionEditor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LionEditor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with LionEditor.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FFTPatcher.Datatypes;
using FFTPatcher.Properties;

namespace FFTPatcher.Editors
{
    public partial class AllItemAttributesEditor : UserControl
    {
        public AllItemAttributes AllItemAttributes { get; private set; }

        public AllItemAttributesEditor()
        {
            InitializeComponent();
            AllItemAttributes = new AllItemAttributes(
                new SubArray<byte>( new List<byte>( Resources.OldItemAttributesBin ), 0 ),
                new SubArray<byte>( new List<byte>( Resources.NewItemAttributesBin ), 0 ) );
            offsetListBox.DataSource = AllItemAttributes.ItemAttributes;
            offsetListBox.SelectedIndexChanged += offsetListBox_SelectedIndexChanged;
            offsetListBox.SelectedIndex = 0;
            offsetListBox_SelectedIndexChanged( offsetListBox, EventArgs.Empty );
        }

        private void offsetListBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            itemAttributeEditor.ItemAttributes = offsetListBox.SelectedItem as ItemAttributes;
        }
    }
}
