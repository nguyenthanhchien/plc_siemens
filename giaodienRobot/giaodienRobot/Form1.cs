using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace giaodienRobot
{
    public partial class Form1 : Form
    {
        SerialPort serialPort; // Đối tượng SerialPort để quản lý kết nối
        Timer colorTimer = new Timer(); // Timer để thay đổi màu của Panel
        int colorStep = 0; // Bước thay đổi màu
        Color targetColor = Color.Green; // Màu xanh khi kết nối thành công
        static double theta1_dht, theta2_dht, theta3_dht,PX_dht,PY_dht,PZ_dht; //  BIẾN value để tính toán 
        static double theta1_dhn, theta2_dhn, theta3_dhn, PX_dhn, PY_dhn, PZ_dhn,Theta_dhn;//biến để giải trong hàm dhn 
        static double L1 = 105, L2 = 162 , L3 = 130, d1 = 270, d3 = 55;
        private Form2 form2;
        public Form1()
        {
            InitializeComponent();
            InitializeComboBoxes();
            InitializeColorTimer();
            //
            this.FormClosed += new FormClosedEventHandler(Form2_FormClosed);
            // Cấu hình giá trị tối thiểu và tối đa cho các slider
            trackBarTheta1.Minimum = -400;
            trackBarTheta1.Maximum = 400;
            trackBarTheta2.Minimum = -400;
            trackBarTheta2.Maximum = 400;
            trackBarTheta3.Minimum = -400;
            trackBarTheta3.Maximum = 400;

            // Đặt giá trị khởi đầu (tùy chọn) cho các slider
            trackBarTheta1.Value = 0;
            trackBarTheta2.Value = 0;
            trackBarTheta3.Value = 0;

            // Khởi tạo 9 textbox để hiển thị giá trị , chỉ được xem , không được sờ 
            textBoxSetTheta1.ReadOnly = true;
            textBoxSetTheta2.ReadOnly = true;
            textBoxSetTheta3.ReadOnly = true;
            textBoxPxF.ReadOnly = true;
            textBoxPyF.ReadOnly = true;
            textBoxPzF.ReadOnly = true;
            textBoxTheta1_IK.ReadOnly = true;
            textBoxTheta2_IK.ReadOnly = true;
            textBoxTheta3_IK.ReadOnly = true;

            // cập nhập và hiển thị khi kéo slider và hiển thị giá trị trong ô textBoxTheta1_dht 
            // textBoxTheta2_dht,textBoxTheta3_dht
            trackBarTheta1.Scroll += new EventHandler(trackBarTheta1_ValueChanged);
            trackBarTheta2.Scroll += new EventHandler(trackBarTheta2_ValueChanged);
            trackBarTheta3.Scroll += new EventHandler(trackBarTheta3_ValueChanged);

            //CÂPJ NHẬP GIÁ TRỊ KHI NHẬP TRONG Ô TEXTBOXTHETA1,2,3_DHT RA SLIDER DHT 
            textBoxTheta1_dht.KeyPress += new KeyPressEventHandler(textBoxTheta1_TextChanged);
            textBoxTheta2_dht.KeyPress += new KeyPressEventHandler(textBoxTheta2_TextChanged);
            textBoxTheta3_dht.KeyPress += new KeyPressEventHandler(textBoxTheta3_TextChanged);

            // cập nhập giá trị khi nhập PX_dhn,PY_dhn,PZ_dhn vào ô và bấm enter 
            textBoxPX_Key.KeyPress += new KeyPressEventHandler(textBoxPX_KeyPress);
            textBoxPY_Key.KeyPress += new KeyPressEventHandler(textBoxPY_KeyPress);
            textBoxPZ_Key.KeyPress += new KeyPressEventHandler(textBoxPZ_KeyPress); 
            textBoxTheta_Key.KeyPress += new KeyPressEventHandler(textBoxTheta_KeyPress);

           

        }

        private void InitializeColorTimer()
        {
            colorTimer.Interval = 100; // Đặt khoảng thời gian giữa các bước thay đổi màu
            colorTimer.Tick += new EventHandler(colorTimer_Tick);
        }
        private void colorTimer_Tick(object sender, EventArgs e)
        {
            if (colorStep < 255)
            {
                int grayValue = 255 - colorStep; // Tính giá trị màu xám ngược
                Color currentColor = Color.FromArgb(255, grayValue, 0); // Chuyển sang màu xanh, thành phần màu xám giảm dần

                // Đặt màu nền cho Panel
                panel1.BackColor = currentColor;

                // Tăng bước thay đổi màu
                colorStep += 5;
            }
            else
            {
                // Khi màu đã đạt màu xanh, dừng Timer
                colorTimer.Stop();
            }
        }
        private void InitializeComboBoxes()
        {
            // Thêm các tùy chọn cho các ComboBox
            string[] ports = SerialPort.GetPortNames();
            comboBoxPortName.Items.AddRange(ports);

            string[] baudRates = { "9600", "19200", "38400", "57600", "115200" };
            comboBoxBaudRate.Items.AddRange(baudRates);

            string[] dataBits = { "8", "7", "6", "5" };
            comboBoxDataBits.Items.AddRange(dataBits);

            string[] parityOptions = { "None", "Odd", "Even", "Mark", "Space" };
            comboBoxParity.Items.AddRange(parityOptions);

            string[] stopBits = { "1", "1.5", "2" };
            comboBoxStopBits.Items.AddRange(stopBits);

            // Thiết lập giá trị mặc định (tùy chọn)
            comboBoxBaudRate.SelectedIndex = 0;
            comboBoxDataBits.SelectedIndex = 0;
            comboBoxParity.SelectedIndex = 0;
            comboBoxStopBits.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem kết nối có tồn tại hay không trước khi tạo mới
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close(); // Đóng kết nối cổng COM nếu đã mở trước đó
                }

                // Lấy giá trị từ các ComboBox đã chọn
                string selectedPortName = comboBoxPortName.SelectedItem.ToString();
                int selectedBaudRate = int.Parse(comboBoxBaudRate.SelectedItem.ToString());
                int selectedDataBits = int.Parse(comboBoxDataBits.SelectedItem.ToString());
                Parity selectedParity = (Parity)Enum.Parse(typeof(Parity), comboBoxParity.SelectedItem.ToString());
                StopBits selectedStopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBoxStopBits.SelectedItem.ToString());

                // Tạo đối tượng SerialPort với các giá trị đã chọn
                serialPort = new SerialPort(selectedPortName, selectedBaudRate, selectedParity, selectedDataBits, selectedStopBits);

                // Mở kết nối
                serialPort.Open();
                button2.BackColor = Color.Orange;
                button1.BackColor = Color.Green;
                // Bắt đầu Timer để thay đổi màu
                colorStep = 0;
                colorTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // static double theta1_dhn, theta2_dhn, theta3_dhn, PX_dhn, PY_dhn, PZ_dhn,Theta_dhn;
        static (double, double, double) cac_IK()
        {
            // Đặt biến ampha ở mức trên cùng của phương thức
            double ampha = 0.0;

            if (PX_dhn > 0 && PY_dhn > 0)
            {
                ampha = Math.Asin(-PY_dhn / (Math.Sqrt(Math.Pow(PX_dhn, 2) + Math.Pow(PY_dhn, 2))));
            }
            else if (PX_dhn < 0 && PY_dhn < 0)
            {
                ampha = Math.Asin(PX_dhn / (Math.Sqrt(Math.Pow(PX_dhn, 2) + Math.Pow(PY_dhn, 2))));
            }

            theta1_dhn = Math.Asin(d3 / (Math.Sqrt(Math.Pow(PX_dhn, 2) + Math.Pow(PY_dhn, 2)))) - ampha;

            theta2_dhn = Math.Asin((PZ_dhn - d1 - L3 * Math.Sin(Theta_dhn)) / L2);
            theta3_dhn = Theta_dhn - theta2_dhn;

            return (theta1_dhn, theta2_dhn, theta3_dhn);
        }
        // nut INVERSE 
        private void button11_Click(object sender, EventArgs e)
        {
            var vitritheta_IK = cac_IK();
            try
            {

                double value1 = vitritheta_IK.Item1;
                double value2 = vitritheta_IK.Item2;
                double value3 = vitritheta_IK.Item3;

                string result1 = $"{value1:N1}";
                string result2 = $"{value2:N1}";
                string result3 = $"{value3:N1}";

                // Cập nhật giá trị trong các TextBox
                textBoxTheta1_IK.Text = result1;
                textBoxTheta2_IK.Text = result2;
                textBoxTheta3_IK.Text = result3;
                // Thực hiện các công việc khác ở đây sau khi gán giá trị

                // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
                MessageBox.Show("Giá trị đã được thiết lập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (form2 == null)
            {
                form2 = new Form2();
                form2.FormClosed += (s, args) => form2 = null; // Đặt form2 thành null khi Form2 đóng
            }
            form2.Show();
            this.Hide(); // Ẩn Form1
        }
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.Owner != null) // Đảm bảo có Owner (Form1)
            {
                this.Owner.Show(); // Hiển thị lại Form1
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
            // Thực hiện các công việc khác ở đây sau khi reset giá trị

            // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
            MessageBox.Show("Các giá trị đã được đặt lại thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        //gởi dữ liệu 
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem serialPort đã mở hay chưa
                if (serialPort != null && serialPort.IsOpen)
                {
                    // Lấy giá trị theta1, theta2, theta3 từ các biến của bạn
                    double theta1 = theta1_dht;
                    double theta2 = theta2_dht;
                    double theta3 = theta3_dht;

                    // Chuyển các giá trị theta thành một chuỗi dạng "theta1,theta2,theta3"
                    string dataToSend = $"{theta1},{theta2},{theta3}\n";


                    // Gửi dữ liệu qua cổng Serial
                    serialPort.WriteLine(dataToSend);

                    // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
                    MessageBox.Show("Dữ liệu đã được gửi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Chưa kết nối với cổng Serial!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //goi du lieu dhn 
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem serialPort đã mở hay chưa
                if (serialPort != null && serialPort.IsOpen)
                {
                    // Lấy giá trị theta1, theta2, theta3 từ các biến của bạn
                    double theta1 = theta1_dhn; 
                    double theta2 = theta2_dhn;
                    double theta3 = theta3_dhn;

                    // Chuyển các giá trị theta thành một chuỗi dạng "theta1,theta2,theta3"
                    string dataToSend = $"{theta1},{theta2},{theta3}\n";

                    // Gửi dữ liệu qua cổng Serial
                    serialPort.WriteLine(dataToSend);

                    // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
                    MessageBox.Show("Dữ liệu đã được gửi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Chưa kết nối với cổng Serial!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // NUT STOP 
        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void trackBarTheta1_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBarTheta3_Scroll(object sender, EventArgs e)
        {

        }
        //GẮP VẬT 
        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem serialPort đã mở hay chưa
                if (serialPort != null && serialPort.IsOpen)
                {
                    

                    // Chuyển các giá trị theta thành một chuỗi dạng "theta1,theta2,theta3"
                    string dataToSend = $"P\n";

                    // Gửi dữ liệu qua cổng Serial
                    serialPort.WriteLine(dataToSend);
                    DROP.BackColor = Color.Red;
                    PICKUP.BackColor = Color.Green;
                    // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
                   // MessageBox.Show("Dữ liệu đã được gửi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Chưa kết nối với cổng Serial!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //NHẢ VẬT 
        private void DROP_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem serialPort đã mở hay chưa
                if (serialPort != null && serialPort.IsOpen)
                {


                    // Chuyển các giá trị theta thành một chuỗi dạng "theta1,theta2,theta3"
                    string dataToSend = $"D\n";

                    // Gửi dữ liệu qua cổng Serial
                    serialPort.WriteLine(dataToSend);
                    DROP.BackColor = Color.Green;
                    PICKUP.BackColor = Color.Red;
                    // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
                    //MessageBox.Show("Dữ liệu đã được gửi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Chưa kết nối với cổng Serial!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = 0;
            trackBarTheta2.Value = 40;
            trackBarTheta3.Value = 30;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = 0;
            trackBarTheta2.Value = 40;
            trackBarTheta3.Value = 26;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            // Đặt giá trị các slider về 0
            trackBarTheta1.Value = -360;
            trackBarTheta2.Value = -360;
            trackBarTheta3.Value = 360;

            // Đặt giá trị các TextBox về rỗng
            textBoxSetTheta1.Text = "";
            textBoxSetTheta2.Text = "";
            textBoxSetTheta3.Text = "";

            // Đặt giá trị các biến về 0 hoặc giá trị mặc định
            theta1_dht = 0;
            theta2_dht = 0;
            theta3_dht = 0;
            PX_dht = 0;
            PY_dht = 0;
            PZ_dht = 0;
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;
            string result1 = $"{value1:N1}";
            string result2 = $"{value2:N1}";
            string result3 = $"{value3:N1}";
            textBoxPxF.Text = result1;
            textBoxPyF.Text = result2;
            textBoxPzF.Text = result3;
            // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
            theta1_dht = trackBarTheta1.Value;
            theta2_dht = trackBarTheta2.Value;
            theta3_dht = trackBarTheta3.Value;

            // Hiển thị giá trị trong các TextBox (tùy chọn)
            textBoxSetTheta1.Text = theta1_dht.ToString();
            textBoxSetTheta2.Text = theta2_dht.ToString();
            textBoxSetTheta3.Text = theta3_dht.ToString();
            //
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    // Đóng kết nối cổng COM nếu đang mở
                    serialPort.Close();
                }
                // Thực hiện các công việc cần thiết khác sau khi đã ngắt kết nối

                // Vô hiệu hóa nút "Disconnect" sau khi đã ngắt kết nối
               
                button1.BackColor = Color.Orange;
                button2.BackColor = Color.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ngắt kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Xử lý sự kiện ValueChanged cho các slider
        private void trackBarTheta1_ValueChanged(object sender, EventArgs e)
        {
            textBoxTheta1_dht.Text = trackBarTheta1.Value.ToString();
        }
        // tinh toan dht 
        static (double, double, double) cac_FK()
        {
            
            PX_dht = L1 * Math.Cos(theta1_dht) + d3 * Math.Sin(theta1_dht) + L2 * Math.Cos(theta1_dht) * Math.Cos(theta2_dht)
                + L3 * Math.Cos(theta1_dht) * Math.Cos(theta2_dht) * Math.Cos(theta3_dht) - L3 * Math.Cos(theta1_dht) * Math.Sin(theta2_dht) * Math.Sin(theta3_dht);
            PY_dht = L1 * Math.Sin(theta1_dht) - d3 * Math.Cos(theta1_dht) + L2 * Math.Sin(theta1_dht) * Math.Cos(theta2_dht)
                + L3 * Math.Sin(theta1_dht) * Math.Cos(theta2_dht) * Math.Cos(theta3_dht) - L3 * Math.Sin(theta1_dht) * Math.Sin(theta2_dht) * Math.Sin(theta3_dht);
            PZ_dht = d1 + L3 * Math.Sin(theta2_dht + theta3_dhn) + L2 * Math.Sin(theta2_dht);

            return (PX_dht, PY_dht, PZ_dht);
        }
        // nhấn Forward xử lý động học thuận 
        private void button10_Click(object sender, EventArgs e)
        {
            var vitri = cac_FK();
            //Console.WriteLine($"{vitri.Item1}, {vitri.Item2}, {vitri.Item3}");
            try
            {
                double value1 = vitri.Item1;
                double value2 = vitri.Item2;
                double value3 = vitri.Item3;

                string result1 = $"{value1:N1}";
                string result2 = $"{value2:N1}";
                string result3 = $"{value3:N1}";
                textBoxPxF.Text = result1;
                textBoxPyF.Text = result2;
                textBoxPzF.Text = result3;    
                // Thực hiện các công việc khác ở đây sau khi gán giá trị

                // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
                //MessageBox.Show("Giá trị đã được thiết lập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void trackBarTheta2_ValueChanged(object sender, EventArgs e)
        {
            textBoxTheta2_dht.Text = trackBarTheta2.Value.ToString();
        }

        private void trackBarTheta3_ValueChanged(object sender, EventArgs e)
        {
            textBoxTheta3_dht.Text = trackBarTheta3.Value.ToString();
        }

        // Xử lý sự kiện TextChanged cho các ô nhập giá trị
        private void textBoxTheta1_TextChanged(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (double.TryParse(textBoxTheta1_dht.Text, out double value))
                {
                    // Giới hạn giá trị nhập vào cho theta1
                    if (value < trackBarTheta1.Minimum)
                        value = trackBarTheta1.Minimum;
                    else if (value > trackBarTheta1.Maximum)
                        value = trackBarTheta1.Maximum;

                    // Cập nhật giá trị của slider
                    trackBarTheta1.Value = (int)value;
                }
            }
        }

        private void textBoxTheta2_TextChanged(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (double.TryParse(textBoxTheta2_dht.Text, out double value))
                {
                    // Giới hạn giá trị nhập vào cho theta2
                    if (value < trackBarTheta2.Minimum)
                        value = trackBarTheta2.Minimum;
                    else if (value > trackBarTheta2.Maximum)
                        value = trackBarTheta2.Maximum;

                    // Cập nhật giá trị của slider
                    trackBarTheta2.Value = (int)value;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void textBoxTheta3_TextChanged(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (double.TryParse(textBoxTheta3_dht.Text, out double value))
                {
                    // Giới hạn giá trị nhập vào cho theta3
                    if (value < trackBarTheta3.Minimum)
                        value = trackBarTheta3.Minimum;
                    else if (value > trackBarTheta3.Maximum)
                        value = trackBarTheta3.Maximum;

                    // Cập nhật giá trị của slider
                    trackBarTheta3.Value = (int)value;
                }
            }
        }
        //double theta1_dhn, theta2_dhn, theta3_dhn, PX_dhn, PY_dhn, PZ_dhn;
        private void textBoxPX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                System.Windows.Forms.TextBox textBoxPX_Key = sender as System.Windows.Forms.TextBox; // Lấy ra TextBox gây ra sự kiện

                if (textBoxPX_Key != null)
                {
                    // Kiểm tra xem giá trị nhập vào có hợp lệ không
                    if (double.TryParse(textBoxPX_Key.Text, out double value))
                    {
                        // Lưu giá trị hoặc thực hiện các tác vụ khác ở đây
                        PX_dhn = (double)value; // lấy giá trị để xử lý tính toán động học 

                        MessageBox.Show("Giá trị đã được lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Giá trị không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void textBoxPY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                System.Windows.Forms.TextBox textBoxPY_Key = sender as System.Windows.Forms.TextBox; // Lấy ra TextBox gây ra sự kiện

                if (textBoxPY_Key != null)
                {
                    // Kiểm tra xem giá trị nhập vào có hợp lệ không
                    if (double.TryParse(textBoxPY_Key.Text, out double value))
                    {
                        // Lưu giá trị hoặc thực hiện các tác vụ khác ở đây
                        PY_dhn = (double)value;
                        MessageBox.Show("Giá trị đã được lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Giá trị không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void textBoxPZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                System.Windows.Forms.TextBox textBoxPZ_Key = sender as System.Windows.Forms.TextBox; // Lấy ra TextBox gây ra sự kiện

                if (textBoxPZ_Key != null)
                {
                    // Kiểm tra xem giá trị nhập vào có hợp lệ không
                    if (double.TryParse(textBoxPZ_Key.Text, out double value))
                    {
                        // Lưu giá trị hoặc thực hiện các tác vụ khác ở đây
                        PZ_dhn = (double)value;
                        MessageBox.Show("Giá trị đã được lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Giá trị không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void textBoxTheta_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                System.Windows.Forms.TextBox textBoxTheta_Key = sender as System.Windows.Forms.TextBox; // Lấy ra TextBox gây ra sự kiện

                if (textBoxTheta_Key != null)
                {
                    // Kiểm tra xem giá trị nhập vào có hợp lệ không
                    if (double.TryParse(textBoxTheta_Key.Text, out double value))
                    {
                        // Lưu giá trị hoặc thực hiện các tác vụ khác ở đây
                        Theta_dhn = (double)value;
                        MessageBox.Show("Giá trị đã được lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Giá trị không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        // nut set theta dht 
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                // Đọc giá trị từ các ô nhập giá trị và slider và gán cho biến theta1, theta2, và theta3
                theta1_dht = trackBarTheta1.Value;
                theta2_dht = trackBarTheta2.Value;
                theta3_dht = trackBarTheta3.Value;

                // Hiển thị giá trị trong các TextBox (tùy chọn)
                textBoxSetTheta1.Text = theta1_dht.ToString();
                textBoxSetTheta2.Text = theta2_dht.ToString();
                textBoxSetTheta3.Text = theta3_dht.ToString();

                // Thực hiện các công việc khác ở đây sau khi gán giá trị

                // Hiển thị thông báo hoặc thực hiện các thao tác cần thiết
                MessageBox.Show("Giá trị đã được thiết lập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



    }
}
