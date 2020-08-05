using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


/*족보
 스+4 다+3 하+2 클+1
 탑숫자*10 
로티플 12000
백티플 11000
스티플 10000
포카드 9000
풀하우스 8000
플러시 7000
마운틴 6000
백스트 5000
스트 4000
트리플 3000
투페 2000
원페 1000
탑 0
J : 11, Q: 12 K:13 A:14


ex. 하2 다2 하J 스J 하5
= 2페어, 스3 탑 = 2000+110+4 = 2114


0: 클로버 2
11: 클로버 King
12:클로버 Ace
13: 하트 2
...
51: 스페이드 Ace

*/
namespace CS_POKER
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PLAYERNUM = 1;
        const int DEALERNUM = 0;
        Queue<int> deck = new Queue<int>();
        List<int> decktmp = new List<int>();
        int cardNum;
        List<int> player = new List<int>();
        List<int> dealer = new List<int>();
        public MainWindow()
        {
            InitializeComponent();
            cardNum = 0;
            deckchk();
            Imagereset();
            deck.Clear();
            player.Clear();
            dealer.Clear();
        }


        private void button_Click_Draw(object sender, RoutedEventArgs e)
        {
            if (cardNum >= 7)
            {

                //경고

                return;
            }
            deckchk();
            cardNum++;
            player.Add(deck.Dequeue());
            paintCard(PLAYERNUM, cardNum, player[cardNum - 1]);
            dealer.Add(deck.Dequeue());
            if (cardNum <= 2 || cardNum == 7) paintBack(0, cardNum);
            else paintCard(0, cardNum, dealer[cardNum - 1]);

            pt.Text = player[cardNum - 1].ToString() + "," + dealer[cardNum - 1].ToString();
            if (cardNum == 7)
            {
                int pscore = pick5(0, PLAYERNUM, 0);
                int dscore = pick5(0, DEALERNUM, 0);
                rank.Text = dscore.ToString() + " vs " + pscore.ToString();
                if (pscore > dscore) debug.Text = "player win";
                else debug.Text = "dealer win";
            }

        }

        
        
        private int pick5(int bitmask,int play,int ret)
        {
            int cnt = 0;
            int cur = 0;
            for(int i=0; i < 7; i++)
            {
                if ((bitmask & 1 << i) > 0) cnt++;

                if (cnt == 5)
                {
                    cur = rank5(bitmask, play);
                    if (cur > ret) return cur;
                    else return ret;
                }
            }
            for(int i=0; i<7;i++)
            {
                int tmp = bitmask;
                if ((tmp | (1 << i)) == 1) continue;
                tmp |= (1 << i);
                ret = pick5(tmp, play, ret);
            }
            return ret;
        }

        private int rank5(int bitmask, int play)
        {
            int ret = 0;
            int[] chk= new int[5];
            int cnt = 0;
            bool flush = false;
            bool straight = false;
            bool bstraight = false;
            bool triple = false;
            bool fourcard = false;
            bool fullhouse = false;
            bool pair1 = false;
            bool pair2 = false;
            int top = -1;
            int tops = -1;
            int tritop = -1;
            int tritops = 2;
            int pairtop = -1;
            int pairtops = -1;

            for (int i=0; i < 7; i++)
            {
                if ((bitmask | (1 << i)) == 1) chk[cnt] = i;
                cnt++;
            }
            cnt = 0;
            if(play== PLAYERNUM)
            {

                //플러시.
                int tmpi = player[0]/13;
                bool tmpb = true;
                for (int i=1; i < 5; i++)
                {
                    if(player[chk[i]] /13 != tmpi)
                    {
                        tmpb = false;
                        break;
                    }
                }
                if (tmpb) flush = true;

                //스트레이트
                int low = player[chk[0]] % 13;
                pairtops = player[chk[0]] / 13;
                tmpb = false;
                bool tmpp = false;
                for(int i=1; i < 5; i++)
                {
                    if (low > player[chk[i]%13]) low = player[chk[i]]%13;
                    else if(low == player[chk[i]]%13)
                    {
                        tmpp = true;
                        pairtop = low;
                        if (pairtops < player[chk[i]] / 13) pairtops = player[chk[i]] / 13;
                        break;
                    }
                }
                if (!tmpp)
                {
                    bool[] tmp5 = new bool[5];
                    for(int i = 0; i < 5; i++)
                    {
                        int c=  player[chk[i]]%13;
                        if (c == low) tmp5[0] = true;
                        else if (c == low + 1) tmp5[1] = true;
                        else if (c == low + 2) tmp5[2] = true;
                        else if (c == low + 3) tmp5[3] = true;
                        else if (c == low + 4) tmp5[4] = true;
                        else break;
                    }
                    if(tmp5[0] && tmp5[1] && tmp5[2] && tmp5[3] && tmp5[4])
                    {
                        straight = true;
                    }
                    // 백스트레이트
                    if(low == 0)
                    {
                        for (int i = 0; i < 5; i++) tmp5[i] = false;
                        for (int i = 0; i < 5; i++)
                        {
                            int c = player[chk[i]] % 13;
                            if (c == low) tmp5[0] = true;
                            else if (c == low + 1) tmp5[1] = true;
                            else if (c == low + 2) tmp5[2] = true;
                            else if (c == low + 3) tmp5[3] = true;
                            else if (c == 12) tmp5[4] = true;
                            else break;
                        }
                        if (tmp5[0] && tmp5[1] && tmp5[2] && tmp5[3] && tmp5[4])
                        {
                            bstraight = true;
                        }
                    }
                }

                //포카드,트리플
                if (tmpp)
                {
                    int tmp = player[chk[0]] % 13;
                    cnt = 0;
                    for(int i=0; i < 5; i++)
                    {
                        if (player[chk[i]] % 13 == tmp)
                        {
                            cnt++;
                            if (player[chk[i]] / 13 == 3) tritops = 3;
                        }
                    }
                    if (cnt == 4) {
                        fourcard = true;
                        tritop = tmp;
                    }
                    else if (cnt == 3)
                    {
                        triple = true;
                        tritop = tmp;
                    }
                    else
                    {
                        cnt = 0;
                        tmp = player[chk[1]] % 13;
                        for (int i = 1; i < 5; i++)
                        {
                            if (player[chk[i]] % 13 == tmp)
                            {
                                cnt++;
                                if (player[chk[i]] / 13 == 3) tritops = 3;
                            }
                        }
                        if (cnt == 4)
                        {
                            fourcard = true;
                            tritop = tmp;
                        }
                        else if (cnt == 3)
                        {
                            triple = true;
                            tritop = tmp;
                        }
                        else
                        {
                            cnt = 0;
                            tmp = player[chk[2]] % 13;
                            for (int i = 2; i < 5; i++)
                            {
                                if (player[chk[i]] % 13 == tmp)
                                {
                                    cnt++;
                                    if (player[chk[i]] / 13 == 3) tritops = 3;
                                }
                            }
                            if (cnt == 3)
                            {
                                triple = true;
                                tritop = tmp;
                            }
                        }
                    }
                }

                //풀하우스
                if (triple)
                {
                    int tmp = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (tmp == -1)
                        {
                            if (tritop != player[chk[i]] % 13) tmp = player[chk[i]] % 13;
                        }
                        else
                        {
                            if (tmp == player[chk[i]] % 13)
                            {
                                fullhouse = true;
                                break;
                            }
                        }
                    }
                }
                //투페어,원페어
                if (tmpp && !triple && !fourcard)
                {
                    int tmp1 = -1;
                    int tmp1s = -1;
                    int tmp2 = -1;
                    int tmp2s = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (pairtop == player[chk[i]] % 13) continue;
                        if (tmp1 == -1)
                        {
                            tmp1 = player[chk[i]] % 13;
                            tmp1s = player[chk[i]] / 13;
                        }
                        else if (tmp2 == -1)
                        {
                            tmp2 = player[chk[i]] % 13;
                            tmp2s = player[chk[i]] / 13;
                        }
                        else
                        {
                            int tmp3 = player[chk[i]] % 13;
                            int tmp3s = player[chk[i]] / 13;
                            if (tmp1 == tmp2)
                            {
                                pair2 = true;
                                if (pairtop < tmp1)
                                {
                                    pairtop = tmp1;
                                    if (tmp1s > tmp2s) pairtops = tmp1s;
                                    else pairtops = tmp2s;
                                }

                            }
                            else if(tmp1 == tmp3)
                            {
                                pair2 = true;
                                if (pairtop < tmp1)
                                {
                                    pairtop = tmp1;
                                    if (tmp1s > tmp3s) pairtops = tmp1s;
                                    else pairtops = tmp3s;
                                }

                            }
                            else if(tmp2 == tmp3)
                            {
                                pair2 = true;
                                if (pairtop < tmp2)
                                {
                                    pairtop = tmp2;
                                    if (tmp2s > tmp3s) pairtops = tmp2s;
                                    else pairtops = tmp3s;
                                }

                            }
                            else
                            {
                                pair1 = true;
                            }
                        }
                    }
                }


                //스트, 탑 용 탑
                top = player[chk[0]] % 13;
                tops = player[chk[0]] / 13;
                for (int i=1; i < 5; i++)
                {
                    if (top < player[chk[i]]%13)
                    {
                        top = player[chk[0]] % 13;
                        tops = player[chk[0]] / 13;
                    }
                }


                //점수 계산
                if (straight && flush && top == 12) ret = 12000 + top * 10 + player[chk[0]] / 13;
                else if (bstraight && flush) ret = 11000 + top * 10 + player[chk[0]] / 13;
                else if (straight && flush) ret = 10000 + top * 10 + player[chk[0]] / 13;
                else if (fourcard) ret = 9000 + tritop * 10 + 4;
                else if (fullhouse) ret = 8000 + tritop * 10 + tritops;
                else if (flush) ret = 7000 + top * 10 + tops;
                else if (straight && top == 12) ret = 6000 + top * 10 + tops;
                else if (bstraight) ret = 5000 + top * 10 + tops;
                else if (straight) ret = 4000 + top * 10 + tops;
                else if (triple) ret = 3000 + tritop * 10 + tritops;
                else if (pair2) ret = 2000 + pairtop * 10 + pairtops;
                else if (pair1) ret = 1000 + pairtop * 10 + pairtops;
                else ret = top * 10 + tops;


            }
            else
            {

            }


            return ret; 


        }

        private void paintBack(int play, int place)
        {
            string str = "Images\\";
            if (play == 1) str+="Red_back";
            else str += "Yellow_back";
            str += ".jpg";

            Uri uri = new Uri(str, UriKind.Relative);
            ImageSource i = new BitmapImage(uri);
            if (play == 1)
            {
                switch (place)
                {
                    case 1: p1.Source = i; break;
                    case 2: p2.Source = i; break;
                    case 3: p3.Source = i; break;
                    case 4: p4.Source = i; break;
                    case 5: p5.Source = i; break;
                    case 6: p6.Source = i; break;
                    case 7: p7.Source = i; break;
                    default: break;
                }
            }
            else
            {
                switch (place)
                {
                    case 1: d1.Source = i; break;
                    case 2: d2.Source = i; break;
                    case 3: d3.Source = i; break;
                    case 4: d4.Source = i; break;
                    case 5: d5.Source = i; break;
                    case 6: d6.Source = i; break;
                    case 7: d7.Source = i; break;
                    default: break;
                }

            }
        }

        private void paintCard(int play,  int place, int card)
        {
            string str ="Images/";
            int num = (card % 13) + 1;
            str += num.ToString();

            switch (card / 13)
            {
                case 0:
                    str += "S";
                    break;
                case 1:
                    str += "D";
                    break;
                case 2:
                    str += "H";
                    break;
                case 3:
                    str += "C";
                    break;
                default: 
                    break;
            }
            str += ".jpg";
            debug.Text = str;

            Uri uri = new Uri(str, UriKind.Relative);
            ImageSource i = new BitmapImage(uri);
            if(play == 1)
            {
                switch (place)
                {
                    case 1: p1.Source = i; break;
                    case 2: p2.Source = i; break;
                    case 3: p3.Source = i; break;
                    case 4: p4.Source = i; break;
                    case 5: p5.Source = i; break;
                    case 6: p6.Source = i; break;
                    case 7: p7.Source = i; break;
                    default: break;
                }
            }
            else
            {
                switch (place)
                {
                    case 1: d1.Source = i; break;
                    case 2: d2.Source = i; break;
                    case 3: d3.Source = i; break;
                    case 4: d4.Source = i; break;
                    case 5: d5.Source = i; break;
                    case 6: d6.Source = i; break;
                    case 7: d7.Source = i; break;
                    default: break;
                }

            }
        }


        private void deckchk()
        {
            if(deck.Count == 0)
            {
                decktmp = shuffle();
                for (int i = 0; i < decktmp.Count; i++)
                {
                    deck.Enqueue(decktmp[i]);
                }
            }
        }

        private void Imagereset()
        {
            Uri uri = new Uri("Images\\blank.jpg",UriKind.Relative);
            ImageSource i = new BitmapImage(uri);
            p1.Source = i;
            p2.Source = i;
            p3.Source = i;
            p4.Source = i;
            p5.Source = i;
            p6.Source = i;
            p7.Source = i;
            d1.Source = i;
            d2.Source = i;
            d3.Source = i;
            d4.Source = i;
            d5.Source = i;
            d6.Source = i;
            d7.Source = i;
        }


        private List<int> shuffle()
        {
            List<int> card = new List<int>();
            for(int i = 0; i < 52; i++)
            {
                card.Add(i);
            }
            Random random = new Random();
            int n = card.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = card[k];
                card[k] = card[n];
                card[n] = value;
            }
            return card;
        }

        private void button_Click_Reset(object sender, RoutedEventArgs e)
        {
            cardNum = 0;
            player.Clear();
            dealer.Clear();
            Imagereset();
            pt.Text = ".";
        }
    }
}
