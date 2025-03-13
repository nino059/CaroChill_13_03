using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CaroChill_13_03.GUI;

namespace CaroChill_13_03.Class
{
    public class ChessBoardManager
    {
        #region Properties
        private Panel chessBoard;
        public Panel ChessBoard
        {
            get { return chessBoard; }
            set { chessBoard = value; }
        }

        private PictureBox avartar1;
        public PictureBox Avatar1
        {
            get { return avartar1; }
            set { avartar1 = value; }
        }
        private PictureBox avartar2;
        public PictureBox Avatar2
        {
            get { return avartar2; }
            set { avartar2 = value; }
        }

        private Label labelName1;
        public Label LabelName1
        {
            get { return labelName1; }
            set { labelName1 = value; }
        }
        private Label labelName2;
        public Label LabelName2
        {
            get { return labelName2; }
            set { labelName2 = value; }
        }

        // Thêm ProgressBar cho hai người chơi
        private ProgressBar progressBar1;
        public ProgressBar ProgressBar1
        {
            get { return progressBar1; }
            set { progressBar1 = value; }
        }

        private ProgressBar progressBar2;
        public ProgressBar ProgressBar2
        {
            get { return progressBar2; }
            set { progressBar2 = value; }
        }

        // Timer cho đếm ngược
        private System.Windows.Forms.Timer gameTimer = new();


        private string imagePathX = $"{Application.StartupPath}\\Resources\\X.png"; // Đường dẫn ảnh X
        private string imagePathO = $"{Application.StartupPath}\\Resources\\O.png"; // Đường dẫn ảnh O

        private Image imageX;
        private Image imageO;

        private List<List<Button>> matrix = new List<List<Button>>();

        public List<Player> Player { get; set; } = new List<Player>();

        private int currentPlayer = 0;
        #endregion

        #region Initialize
        public ChessBoardManager(Panel chessBoard, PictureBox avt1, PictureBox avt2, Label lab1, Label lab2, ProgressBar prgBar1, ProgressBar prgBar2)
        {
            this.chessBoard = chessBoard;
            this.avartar1 = avt1;
            this.avartar2 = avt2;
            this.labelName1 = lab1;
            this.labelName2 = lab2;
            this.progressBar1 = prgBar1;
            this.progressBar2 = prgBar2;


            imageX = File.Exists(imagePathX) ? Image.FromFile(imagePathX) : new Bitmap(1, 1);
            imageO = File.Exists(imagePathO) ? Image.FromFile(imagePathO) : new Bitmap(1, 1);

            // Khởi tạo người chơi
            Player.Add(new Player("Player 1", imageX));
            Player.Add(new Player("Player 2", imageO));
            // Khởi tạo timer và ProgressBar
            InitializeTimer();
            SetupProgressBars();
        }

        /// <summary>
        /// Khởi tạo timer đếm ngược
        /// </summary>
        private void InitializeTimer()
        {
            gameTimer = new System.Windows.Forms.Timer
            {
                Interval = 100 // Cập nhật mỗi 100ms để có hiệu ứng mượt mà
            };
            gameTimer.Tick += GameTimer_Tick;
        }

        /// <summary>
        /// Thiết lập các ProgressBar
        /// </summary>
        private void SetupProgressBars()
        {
            // Thiết lập ProgressBar cho người chơi 1
            if (progressBar1 != null)
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = Cons.TIME_LIMIT * 10; // *10 vì interval là 100ms
                progressBar1.Value = progressBar1.Maximum;
                progressBar1.Step = 1;
            }

            // Thiết lập ProgressBar cho người chơi 2
            if (progressBar2 != null)
            {
                progressBar2.Minimum = 0;
                progressBar2.Maximum = Cons.TIME_LIMIT * 10;
                progressBar2.Value = progressBar2.Maximum;
                progressBar2.Step = 1;
            }
        }
        #endregion

        #region Timer Methods
        /// <summary>
        /// Xử lý sự kiện Tick của Timer
        /// </summary>
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            // Lấy ProgressBar của người chơi hiện tại
            ProgressBar currentProgressBar = currentPlayer == 0 ? progressBar1 : progressBar2;

            // Giảm giá trị ProgressBar
            if (currentProgressBar != null && currentProgressBar.Value > 0)
            {
                currentProgressBar.Value--;

                // Đổi màu khi thời gian gần hết ( Không chạy )
                if (currentProgressBar.Value <= currentProgressBar.Maximum / 2)
                {
                    try
                    {
                        currentProgressBar.ForeColor = Color.Red;
                    }
                    catch { /* Bỏ qua nếu không thay đổi được màu */ }
                }
            }

            // Xử lý khi hết thời gian
            if (currentProgressBar != null && currentProgressBar.Value == 0)
            {
                gameTimer.Stop();
                // nếu formChessBoard mà hiện thì mới thông báo
                if (frmChessBoard.ActiveForm == null || frmChessBoard.ActiveForm.Visible == false)
                {
                    MessageBox.Show($"{Player[currentPlayer].Name} đã thua do hết thời gian!",
                                   "Hết thời gian", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                // Reset bàn cờ
                DrawChessBoard();
            }
        }

        /// <summary>
        /// Bắt đầu đếm ngược cho lượt hiện tại
        /// </summary>
        private void StartCountdown()
        {
            // Xác định ProgressBar hiện tại và ProgressBar của đối thủ
            ProgressBar currentProgressBar = currentPlayer == 0 ? progressBar1 : progressBar2;
            ProgressBar otherProgressBar = currentPlayer == 0 ? progressBar2 : progressBar1;

            // Reset ProgressBar hiện tại
            if (currentProgressBar != null)
            {
                currentProgressBar.Visible = true;
            }

            // Reset ProgressBar của đối thủ
            if (otherProgressBar != null)
            {
                otherProgressBar.Visible = false;
                otherProgressBar.Value = otherProgressBar.Maximum;

            }

            // Bắt đầu đếm ngược
            gameTimer.Start();
        }

        /// <summary>
        /// Dừng timer khi đóng form hoặc kết thúc trò chơi
        /// </summary>
        public void StopTimer()
        {
            if (gameTimer != null && gameTimer.Enabled)
            {
                gameTimer.Stop();
            }
        }
        #endregion

        #region Methods
        public void DrawChessBoard()
        {
            // Dừng timer nếu đang chạy
            gameTimer.Stop();

            // Xóa bàn cờ cũ
            chessBoard.Controls.Clear();
            // Khởi tạo ma trận button
            matrix.Clear();

            for (int i = 0; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                matrix.Add(new List<Button>());

                for (int j = 0; j < Cons.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Cons.CHESS_WIDTH,
                        Height = Cons.CHESS_HEIGHT,
                        Location = new Point(j * Cons.CHESS_WIDTH, i * Cons.CHESS_HEIGHT),
                        BackgroundImageLayout = ImageLayout.Stretch, // Căn chỉnh ảnh vừa với button
                        Tag = i.ToString(), // Lưu vị trí dòng để dễ xử lý sau này
                        //FlatStyle = FlatStyle.Flat
                    };

                    btn.Click += Btn_Click; // Thêm sự kiện click

                    chessBoard.Controls.Add(btn);
                    matrix[i].Add(btn); // Thêm button vào ma trận
                }
            }

            // Reset màu nền của tên người chơi
            labelName1.BackColor = Color.White;
            labelName2.BackColor = Color.White;

            // Thiết lập người chơi đầu tiên
            currentPlayer = 0;
            ChangePlayer();

            // Reset ProgressBar
            SetupProgressBars();

            // Bắt đầu đếm ngược cho người chơi đầu tiên
            StartCountdown();
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            Button? btn = sender as Button;

            if (btn == null) return;

            // Nếu button đã có ảnh thì không xử lý
            if (btn.BackgroundImage != null)
                return;

            // Dừng timer hiện tại
            gameTimer.Stop();

            // Hiển thị ảnh X hoặc O tùy thuộc vào người chơi hiện tại
            btn.BackgroundImage = Player[currentPlayer].Mark;

            // Chuyển lượt người chơi
            currentPlayer = currentPlayer == 0 ? 1 : 0;

            // Cập nhật giao diện để hiển thị lượt người chơi hiện tại
            ChangePlayer();

            // Kiểm tra thắng thua
            CheckWin(btn);

            // Bắt đầu đếm ngược cho người chơi mới
            StartCountdown();
        }

        private void ChangePlayer()
        {
            // Reset màu nền
            labelName1.BackColor = Color.White;
            labelName2.BackColor = Color.White;

            // Thay đổi hiển thị tên người chơi hiện tại
            if (currentPlayer == 0)
            {
                labelName1.BackColor = Color.Green;
            }
            else
            {
                labelName2.BackColor = Color.Green;
            }
        }

        private void CheckWin(Button btn)
        {
            // Lấy vị trí của button trong ma trận
            int row = Convert.ToInt32(btn.Tag);
            int col = matrix[row].IndexOf(btn);

            // Phương thức này sẽ được phát triển sau để kiểm tra thắng thua
            // Dựa trên các quy tắc của trò chơi Caro
            // Có thể kiểm tra theo hàng, cột, đường chéo
        }

        public void StartGame()
        {
            // Dừng timer nếu đang chạy
            gameTimer.Stop();

            // Reset bàn cờ nếu cần
            if (matrix.Count > 0)
            {
                foreach (List<Button> buttonRow in matrix)
                {
                    foreach (Button button in buttonRow)
                    {
                        button.BackgroundImage = null;
                    }
                }

                // Reset màu nền
                labelName1.BackColor = Color.White;
                labelName2.BackColor = Color.White;

                // Thiết lập người chơi đầu tiên
                currentPlayer = 0;
                ChangePlayer();

                // Reset ProgressBar
                SetupProgressBars();

                // Bắt đầu đếm ngược
                StartCountdown();
            }
            else
            {
                DrawChessBoard();
            }
        }
        #endregion
    }
}
