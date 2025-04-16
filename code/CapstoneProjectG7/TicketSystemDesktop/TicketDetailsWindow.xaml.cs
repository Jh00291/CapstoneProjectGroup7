﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop
{
    /// <summary>
    /// Interaction logic for TicketDetailsWindow.xaml
    /// </summary>
    public partial class TicketDetailsWindow : Window
    {
        public TicketDetailsWindow(Ticket ticket)
        {
            InitializeComponent();
            DataContext = ticket;
        }
    }

}
