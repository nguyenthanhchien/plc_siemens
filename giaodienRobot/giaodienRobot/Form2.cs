using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace giaodienRobot
{
    
    public partial class Form2 : Form
    {
        private Form1 form1; // Khai báo biến form1 ở mức lớp
        public Form2()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
        // chèn ảnh và resize kích thước cho phù hợp với màn hình 
        

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            

        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.Owner != null) // Đảm bảo có Owner (Form1)
            {
                this.Owner.Show(); // Hiển thị lại Form1
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            
        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (form1 == null)
            {
                form1 = new Form1();
                form1.FormClosed += (s, args) => form1 = null; // Đặt form2 thành null khi Form2 đóng
            }
            form1.Show();
            this.Hide(); // Ẩn Form1
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
