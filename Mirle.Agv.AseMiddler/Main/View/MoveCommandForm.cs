﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.AseMiddler.Controller;
using Mirle.Agv.AseMiddler.Model;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class MoveCommandForm : Form
    {
        protected MapInfo theMapInfo;

        public MoveCommandForm()
        {
            InitializeComponent();
        }

        public MoveCommandForm( MapInfo mapInfo) : this()
        {
         
            theMapInfo = mapInfo;
        }        
    }    
}
