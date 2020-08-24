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
2:0 ..10:8 J : 9, Q: 10 K:11 A:12


ex. 하2 다2 하J 스J 하5
= 2페어, 스3 탑 = 2000+110+1 = 2111


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
        private int max(int a, int b) { if (a > b) return a; return b; }


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
           // if (cardNum <= 2 || cardNum == 7) paintBack(0, cardNum);
            //else 
                paintCard(0, cardNum, dealer[cardNum - 1]);

            pt.Text = player[cardNum - 1].ToString() + "," + dealer[cardNum - 1].ToString();
            if (cardNum == 7)
            {
                /*
                    int pscore = pick5(0, PLAYERNUM, 0);
                    int dscore = pick5(0, DEALERNUM, 0);
                */

                int pscore = rank5(0b11111, 1);
                int dscore = rank5(0b11111, 0);
                string str = "";
                for (int i = 0; i < 5; i++)
                {
                    str += (player[i] % 13).ToString();
                    if (i != 5) str += ",";
                }
                debug.Text ="(Dealer)" +rankstr(dscore) +"\nvs\n(Player)" +rankstr(pscore);
                
                if (pscore > dscore) rank.Text = "player win";
                else rank.Text = "dealer win";

                //debug here


            }


        }
        private string rankstr(int score)
        {
            string retstr = "";
            int ranktmp = score / 1000;
            int topstmp = score%10;
            int toptmp = (score - ranktmp * 1000 - topstmp)/10;

            switch (ranktmp)
            {
                case 12:
                    retstr += "Royal Straight Flush,";
                    break;
                case 11:
                    retstr += "Back Straight Flush,";
                    break;
                case 10:
                    retstr += "Straight Flush,";
                    break;
                case 9:
                    retstr += "Four Card,";
                    break;
                case 8:
                    retstr += "Full House,";
                    break;
                case 7:
                    retstr += "Flush,";
                    break;
                case 6:
                    retstr += "Mountain,";
                    break;
                case 5:
                    retstr += "Back Straight,";
                    break;
                case 4:
                    retstr += "Straight,";
                    break;
                case 3:
                    retstr += "Triple,";
                    break;
                case 2:
                    retstr += "Two Pairs,";
                    break;
                case 1:
                    retstr += "One Pair,";
                    break;
                default:
                    break;
            }

            if (topstmp == 3) retstr += "Spade ";
            else if (topstmp == 2) retstr += "Diamond ";
            else if (topstmp == 1) retstr += "Heart ";
            else if (topstmp == 0) retstr += "Clover ";


            if (toptmp == 12) retstr += "Ace";
            else if (toptmp == 11) retstr += "King";
            else if (toptmp == 10) retstr += "Queen";
            else if (toptmp == 9) retstr += "Jack";
            else retstr += (toptmp + 2).ToString();

            retstr += " Top";

            return retstr;
        }
        
        
        private int pick5(int bitmask,int play,int score)
        {
            int cnt = 0;
            int cur = 0;
            int tmp = bitmask;
            while (tmp > 0)
            {
                cnt += (tmp % 2);
                tmp /= 2;
            }
            if (cnt >= 5)
            {
                cur = rank5(bitmask, play);
                if (cur > score) return cur;
                else return score;
            }
            for(int i=0; i<7;i++)
            {
                tmp = bitmask;
                if ((tmp | (1 << i)) == 1) continue;
                tmp |= (1 << i);
                int tmps = pick5(tmp, play, score);
                score = tmps;
            }
            return score;
        }

        private int rank5(int bitmask, int play)
        {
            int ret = 0;
            int[] chk= new int[5];
            int cnt = 0;
            // flags 
            bool flush = false;
            bool straight = false;
            bool bstraight = false;
            bool triple = false;
            bool fourcard = false;
            bool fullhouse = false;
            bool pair1 = false;
            bool pair2 = false;
            //***top =number,  ***tops= shape
            int top = -1;
            int tops = -1;
            int tritop = -1;       //트리플,포카드 시 사용.
            int tritops = 2;
            int pairtop = -1;
            int pairtops = -1;

            // * 모든 카드는 player[chk[i]] 꼴로 사용되어야 함.
            // * player[chk[i]]/13 : 문양. player[chk[i]]%13 : 숫자.

            //어떤 카드를 쓸지 확인.
            for (int i=0; i < 7; i++)
            {
                if ((bitmask & (1 << i)) == (1<<i)) chk[cnt] = i;
                cnt++;
            }
            cnt = 0;
            if(play== PLAYERNUM)
            {

                //플러시.
                int tmpi = player[chk[0]]/13;
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
                bool tmpp = false;  // 페어 여부.
                for(int i=1; i < 5; i++)    // 제일 낮은 숫자 확인.
                {
                    if (low > player[chk[i]] % 13) low = player[chk[i]] % 13;
                    else if (low == player[chk[i]] % 13) // 같은 숫자가 하나라도 있으면
                    {
                        tmpp = true;
                        pairtop = low;
                        if (pairtops < player[chk[i]] / 13) pairtops = player[chk[i]] / 13;
                        break;
                    }
                }
                if (!tmpp) //페어 미검출시
                {
                    bool[] tmp5 = new bool[5];
                    for (int i = 0; i < 5; i++) tmp5[i] = false;
                    for (int i = 0; i < 5; i++)
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
                    if(low == 0) //0,1,2,3,12
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
                if (tmpp) //페어 이상 존재시.
                {
                    int tmp = player[chk[0]] % 13; //0번과 같은 4개가 있는지 확인.
                    cnt = 0;
                    for(int i=0; i < 5; i++)
                    {
                        if (player[chk[i]] % 13 == tmp)
                        {
                            cnt++;
                            if (player[chk[i]] / 13 == 3) tritops = 3; //스페이드 존재시 스페이드가 무조건 최고 문양. 그외에는 무조건 다이아.
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
                if (triple) //트리플인지 먼저 확인.
                {
                    int tmp = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (tmp == -1)
                        {
                            if (tritop != player[chk[i]] % 13) tmp = player[chk[i]] % 13; //트리플의 탑과 카드가 다를경우 갱신.
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
                if (!triple && !fourcard)
                {
                    int tmp1 = -1; // 원페어 위치.
                    pairtop = -1; //페어 탑 숫자
                    pairtops = -1; //페어 탑 모양.
                    int[] pchk = new int[5];
                    for (int i = 0; i < 5; i++) pchk[i] = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (tmp1 == i) continue; //원페어에 이미 포함된 경우 넘어감.

                        for(int j=i+1; j < 5; j++)
                        {
                            if(player[chk[i]]%13 == player[chk[j]] % 13)
                            {
                                if (!pair1)
                                {
                                    // 먼저 검색된 원페어
                                    pair1 = true;
                                    tmp1 = j;
                                    pairtop = player[chk[i]] % 13;
                                    pairtops = max(player[chk[i]] / 13, player[chk[j]] / 13);
                                    break;
                                }
                                else
                                {
                                    //투페어 탐색
                                    pair2 = true;
                                    if(player[chk[j]]%13 > pairtop) // 두번째 페어가 더 클 경우 탑 갱신
                                    {
                                        pairtop = player[chk[i]] % 13;
                                        pairtops = max(player[chk[i]] / 13, player[chk[j]] / 13);
                                    }
                                }
                            }
                        }
                    }
                }

                //string str = "";

                //스트, 탑 용 탑
                top = player[chk[0]] % 13;
                tops = player[chk[0]] / 13;
                for (int i=0; i < 5; i++)
                {
                    if (top < (player[chk[i]] % 13))
                    {
                        top = player[chk[i]] % 13;
                        tops = player[chk[i]] / 13;
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

                //str += "//";
                //for(int i=0; i < 5; i++)
                //{
                //    str += (player[chk[i]] % 13).ToString();
                //    str+= "("+(player[chk[i]] / 13).ToString()+")";
                //    if (i != 5) str += ",";
                //}

                //str+= "///"+"s:" + straight.ToString() + "/f:" + flush.ToString() + "/b:" + bstraight.ToString() + "/4:" + fourcard.ToString() + "/f:" + fullhouse.ToString() + "/t:" + triple.ToString() + "/P:" + pair2.ToString() + "/p:" + pair1.ToString() + "///top:" + (top).ToString() + ",low:"+low.ToString();
                //str += "///" + ret.ToString();
                //debug.Text = str;
            }
            else
            {
                //플러시.
                int tmpi = dealer[chk[0]] / 13;
                bool tmpb = true;
                for (int i = 1; i < 5; i++)
                {
                    if (dealer[chk[i]] / 13 != tmpi)
                    {
                        tmpb = false;
                        break;
                    }
                }
                if (tmpb) flush = true;

                //스트레이트
                int low = dealer[chk[0]] % 13;
                pairtops = dealer[chk[0]] / 13;
                tmpb = false;
                bool tmpp = false;  // 페어 여부.
                for (int i = 1; i < 5; i++)    // 제일 낮은 숫자 확인.
                {
                    if (low > dealer[chk[i]] % 13) low = dealer[chk[i]] % 13;
                    else if (low == dealer[chk[i]] % 13) // 같은 숫자가 하나라도 있으면
                    {
                        tmpp = true;
                        pairtop = low;
                        if (pairtops < dealer[chk[i]] / 13) pairtops = dealer[chk[i]] / 13;
                        break;
                    }
                }
                if (!tmpp) //페어 미검출시
                {
                    bool[] tmp5 = new bool[5];
                    for (int i = 0; i < 5; i++) tmp5[i] = false;
                    for (int i = 0; i < 5; i++)
                    {
                        int c = dealer[chk[i]] % 13;
                        if (c == low) tmp5[0] = true;
                        else if (c == low + 1) tmp5[1] = true;
                        else if (c == low + 2) tmp5[2] = true;
                        else if (c == low + 3) tmp5[3] = true;
                        else if (c == low + 4) tmp5[4] = true;
                        else break;
                    }
                    if (tmp5[0] && tmp5[1] && tmp5[2] && tmp5[3] && tmp5[4])
                    {
                        straight = true;
                    }
                    // 백스트레이트
                    if (low == 0) //0,1,2,3,12
                    {
                        for (int i = 0; i < 5; i++) tmp5[i] = false;
                        for (int i = 0; i < 5; i++)
                        {
                            int c = dealer[chk[i]] % 13;
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
                if (tmpp) //페어 이상 존재시.
                {
                    int tmp = dealer[chk[0]] % 13; //0번과 같은 4개가 있는지 확인.
                    cnt = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (dealer[chk[i]] % 13 == tmp)
                        {
                            cnt++;
                            if (dealer[chk[i]] / 13 == 3) tritops = 3; //스페이드 존재시 스페이드가 무조건 최고 문양. 그외에는 무조건 다이아.
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
                        tmp = dealer[chk[1]] % 13;
                        for (int i = 1; i < 5; i++)
                        {
                            if (dealer[chk[i]] % 13 == tmp)
                            {
                                cnt++;
                                if (dealer[chk[i]] / 13 == 3) tritops = 3;
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
                            tmp = dealer[chk[2]] % 13;
                            for (int i = 2; i < 5; i++)
                            {
                                if (dealer[chk[i]] % 13 == tmp)
                                {
                                    cnt++;
                                    if (dealer[chk[i]] / 13 == 3) tritops = 3;
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
                if (triple) //트리플인지 먼저 확인.
                {
                    int tmp = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (tmp == -1)
                        {
                            if (tritop != dealer[chk[i]] % 13) tmp = dealer[chk[i]] % 13; //트리플의 탑과 카드가 다를경우 갱신.
                        }
                        else
                        {
                            if (tmp == dealer[chk[i]] % 13)
                            {
                                fullhouse = true;
                                break;
                            }
                        }
                    }
                }
                //투페어,원페어
                if (!triple && !fourcard)
                {
                    int tmp1 = -1; // 원페어 위치.
                    pairtop = -1; //페어 탑 숫자
                    pairtops = -1; //페어 탑 모양.
                    int[] pchk = new int[5];
                    for (int i = 0; i < 5; i++) pchk[i] = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (tmp1 == i) continue; //원페어에 이미 포함된 경우 넘어감.

                        for (int j = i + 1; j < 5; j++)
                        {
                            if (dealer[chk[i]] % 13 == dealer[chk[j]] % 13)
                            {
                                if (!pair1)
                                {
                                    // 먼저 검색된 원페어
                                    pair1 = true;
                                    tmp1 = j;
                                    pairtop = dealer[chk[i]] % 13;
                                    pairtops = max(dealer[chk[i]] / 13, dealer[chk[j]] / 13);
                                    break;
                                }
                                else
                                {
                                    //투페어 탐색
                                    pair2 = true;
                                    if (dealer[chk[j]] % 13 > pairtop) // 두번째 페어가 더 클 경우 탑 갱신
                                    {
                                        pairtop = dealer[chk[i]] % 13;
                                        pairtops = max(dealer[chk[i]] / 13, dealer[chk[j]] / 13);
                                    }
                                }
                            }
                        }
                    }
                }

                string str = "";

                //스트, 탑 용 탑
                top = dealer[chk[0]] % 13;
                tops = dealer[chk[0]] / 13;
                for (int i = 0; i < 5; i++)
                {
                    if (top < (dealer[chk[i]] % 13))
                    {
                        top = dealer[chk[i]] % 13;
                        tops = dealer[chk[i]] / 13;
                    }
                }


                //점수 계산
                if (straight && flush && top == 12) ret = 12000 + top * 10 + dealer[chk[0]] / 13;
                else if (bstraight && flush) ret = 11000 + top * 10 + dealer[chk[0]] / 13;
                else if (straight && flush) ret = 10000 + top * 10 + dealer[chk[0]] / 13;
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

                str += "//";
                for (int i = 0; i < 5; i++)
                {
                    str += (dealer[chk[i]] % 13).ToString();
                    str += "(" + (dealer[chk[i]] / 13).ToString() + ")";
                    if (i != 5) str += ",";
                }

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
            int num = (card % 13) + 2;
            if (num == 14) num = 1;
            str += num.ToString();

            switch (card / 13)
            {
                case 0:
                    str += "C";
                    break;
                case 1:
                    str += "H";
                    break;
                case 2:
                    str += "D";
                    break;
                case 3:
                    str += "S";
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
