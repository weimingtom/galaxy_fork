using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace SilverlightApplication1
{
    public partial class Game : UserControl
    {
        //Dictionary<string,object>  _vars;
        Dictionary<string,int> _vars;
        XElement _root;
        XElement _currSection;
        MediaElement _currSound;
        //int _currSectionId;
        string _nextSectionId;
       // string _currItemId;
        int _numSections;
        bool hasSelect = false;
        bool isIf = false;

        public Game()
        {
            // Required to initialize variables
            InitializeComponent();
           // _vars = new Dictionary<string,object> ();
            _vars = new Dictionary<string, int>();
            LoadScene("script/scene4.xml");
            InitScene();
        }

        //创建场景
        public void InitScene()
        {
            ImageBrush bgImg = new ImageBrush();
            bgImg.ImageSource = new BitmapImage((new Uri("image/scene/" + _root.Attribute("bgimg").Value, UriKind.RelativeOrAbsolute)));
            Scene.Background = bgImg;
            Bgm.Source = new Uri("sound/bgm/" + _root.Attribute("bgm").Value, UriKind.RelativeOrAbsolute);
            Bgm.Play();
            _currSound = Bgm;
            _numSections = _root.Elements("section").Count();
            //Var
            foreach( var v in _root.Elements("var-def"))
            {
                switch (v.Attribute("type").Value)
                {
                    case "int":
                        _vars.Add(v.Attribute("name").Value, 0);
                        break;
                        /*
                    case "string":
                        string temp = null;
                        _vars.Add(v.Attribute("name").Value,temp);
                        break;
                    case "float":
                        _vars.Add(v.Attribute("name").Value, new Double());
                        break;
                         * */
                }
            }
            //_currSectionId = 0;
            //_imgX = _imgY = 0;
            /*Img.Visibility = Visibility.Collapsed;
            Say.Visibility = Visibility.Collapsed;*/
            GoToSection("0");
        }

        public void LoadScene(string dir)
        {
            /*
            Uri uri = new Uri(dir,UriKind.RelativeOrAbsolute);
		    WebClient client = new WebClient();
			client.OpenReadCompleted += onSceneLoaded;
			client.OpenReadAsync(uri);
             * */

            //Load
            XDocument doc = XDocument.Load(dir);
            _root = doc.Element("scene");
            
            // InitScene();
        }

        /*
        void onSceneLoaded(object sender,OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //lbl.Text = e.Error.Message;
                System.Console.WriteLine(e.Error.Message);
                return;
            }
			
            using (Stream s = e.Result)
            {
                XDocument doc = XDocument.Load(s);
                _root = doc.Element("scene");
            }
            InitScene();
        }
         * */

        void OnClick(object sender, RoutedEventArgs e)
        {
            //点击事件转换section之前需要数停止正在播放的Sound，但Bgm不停止。
            /*
            if (Sound.CurrentState != MediaElementState.Stopped)
            {
                Sound.Stop();
                _currSound = Bgm;
                _currSound.Play();
            }*/

            /*
            //GoToSection("1");//Test!
            if (_currSectionId++ <= _numSections)
            {
                GoToSection(Convert.ToString(_currSectionId++));
            }
            else
            {
                MessageBox.Show("_currSectionId大于对话数量！");
            }
             * */
			if(!hasSelect)
			{
                GoToSection(_nextSectionId);
			}
        }

        void OnSoundEnded(object sender, RoutedEventArgs e)
        {
            _currSound.Stop();
            _currSound = Bgm;
            _currSound.Play();
        }

        void onItemClick(object sender, RoutedEventArgs e)
        {
            //string currItemId = (sender as Button).Tag as string;
            _nextSectionId = (sender as Button).Tag as string;
            GoToSection(_nextSectionId);
        }

        //这个是进入下一个对话段的动作
        public void GoToSection(string s_id)
        {
            _currSection = (from el in _root.Elements() where ((el.Name == "section") && (el.Attribute("id").Value == s_id)) select el).First();
            hasSelect = false;
            isIf = false;
            //_currSectionId = Convert.ToInt32(_currSection.Attribute("id").Value);
            Img.Visibility = Visibility.Collapsed;
            Say.Visibility = Visibility.Collapsed;
            bool findImg = false;
            bool findSay = false;
            foreach (XElement e in _currSection.Elements())
            {
                #region Say
                if (e.Name == "say")
                {
                    //清空所有的按钮
                    List<UIElement> btnsList = new List<UIElement>();
                    foreach (UIElement v in Say.Children)
                    {
                    	if (v is Button)
                    	{
                    		btnsList.Add(v);
                    	}
                    }
                    UIElement[] btns = btnsList.ToArray();
                    bool hasBtn = (btns==null) ? false : true;
                    if (hasBtn)
                    {
                        //bool r;
                        foreach (var btn in btns)
                        {
                        	Say.Children.Remove(btn);
                        	/*
                            r = Say.Children.Remove(btn);
                            if (!r)
                            {
                                MessageBox.Show("未能清除按钮");
                                return;
                            }*/
                        }
                    }
                    //报告发现Say标签
                    findSay = true;
                    //Say.Visibility = Visibility.Visible;
                    if (e.Element("select-title")!=null)
                    {
                        SayText.Text = e.Element("select-title").Value;
                    }
                    else
                    {
                        SayText.Text = e.Value;
                    }
                    if (SayTitle.Text != e.Attribute("speaker").Value)
                    {
                        SayTitle.Text = e.Attribute("speaker").Value;
                    }
                    if (e.Element("select") != null)
                    {
                        //
                        hasSelect = true;
                        Say.Width = 300;
						Say.Height = 0;
                        SayTitle.Width = 300;
                        SayTitle.Height = 20;
                        SayText.Width = 300;
                        SayText.Height = 50;
                        //Say.Height = Say.Height += 200;
                        int numItems = e.Element("select").Elements("item").Count();
                        double btnHeight = 50;
                        foreach (var item in (e.Element("select").Elements()))
                        {
                            Button btn = new Button();
                            btn.Width = Say.Width;
                            btn.Height = btnHeight;
                            btn.Content = item.Value;
							btn.Foreground = new SolidColorBrush(Colors.White);
							//btn.Margin = new Thickness(0,0,0,1);
                           
                            //"{StaticResource ButtonStyle1}"
							//btn.Style.; 
                            //btn.RenderTransformOrigin = new Point(0,0);
                            //TranslateTransform btnTrans = new TranslateTransform();
                            //btnTrans.X = SayTrans.X;
                            //btnTrans.Y = SayTrans.Y + SayTitle.Height + SayText.Height + btn.Height * btnIndex;
                            btn.Style = Scene.Resources["ButtonStyle1"] as Style;
                            btn.FontFamily = new FontFamily("Arial Black");
                            btn.FontSize = 16;
                            btn.Opacity = 0.9;
                            btn.HorizontalAlignment = HorizontalAlignment.Left;
                            btn.VerticalAlignment = VerticalAlignment.Top;
                            btn.Click += onItemClick;
                            //把id属性储存到按钮的Tag中，以便onItemClick事件处理函数能分辨是哪个按钮按了
                            //btn.Tag = item.Attribute("id").Value;
                            //还不如直接把next-section放进去
                            btn.Tag = item.Attribute("next-section").Value;
                            Say.Children.Add(btn);
                        }
                        /*
                        TextBlock bottom = new TextBlock();
                        bottom.Width = 300;
                        bottom.Height = 10;
                        bottom.Text = "";
                        Say.Children.Add(bottom);*/
                        Say.Height += SayTitle.ActualHeight + SayText.ActualHeight + btnHeight * numItems;
                        SayTrans.X = Scene.Width / 2 - Say.Width / 2;
                        SayTrans.Y = Scene.Height / 2 - Say.Height / 2;
                    }
                    else
                    {
                        Say.Height = 132;
                        Say.Width = 450;
                        SayTrans.X = 70;
                        SayTrans.Y = 295;
                    }
                }
                #endregion
                #region Img
                if (e.Name == "img")
                {
                    findImg = true;
                    //Img.Visibility = Visibility.Visible;
                    #region Source
                    if (e.Value != Img.Source.ToString())
                    {
                        Img.Source = new BitmapImage(new Uri("image/" + e.Value, UriKind.RelativeOrAbsolute));
                    }
                    if (e.Attribute("source") != null)
                    {
                        Img.Source = new BitmapImage(new Uri("image/" + e.Attribute("source").Value, UriKind.RelativeOrAbsolute));
                    }
                    #endregion
                    #region ImgXYWH
                    if (e.Attribute("bust") != null)
                    {
                        bool b = Convert.ToBoolean(e.Attribute("bust").Value);
                        if (b)
                        {
                            Img.Width = 300;
                            Img.Height = 370;
                            ImgPosTrans.X = Say.Width / 2 - 150;
                            ImgPosTrans.Y = SayTrans.Y - Img.Height;
                        }
                    }
                    else
                    {
                        if (e.Attribute("x") != null)
                        {
                            /*
                            double dX = Convert.ToDouble(e.Attribute("x").Value);
                            if (dX < _imgX)
                            {
                                ImgPosTrans.X = _imgX - Math.Abs(dX - _imgX);
                                _imgX = dX;
                                Status.Content = "ImgX:" + _imgX.ToString() + "TransX:" + ImgPosTrans.X.ToString();
                            }
                            else if(dX>_imgX)
                            {
                                ImgPosTrans.X = _imgX + Math.Abs(dX - _imgX);
                                _imgX = dX;
                                Status.Content = "ImgX:" + _imgX.ToString() + "TransX:" + ImgPosTrans.X.ToString();}*/
                            ImgPosTrans.X = Convert.ToDouble(e.Attribute("x").Value);
                        }
                        else if (e.Element("x") != null)
                        {
                            /*
                            double dX = Convert.ToDouble(e.Element("x").Value);
                            if (dX < _imgX)
                            {
                                ImgPosTrans.X = _imgX - Math.Abs(dX - _imgX);
                                _imgX = dX;
                                Status.Content = "ImgX:" + _imgX.ToString() + "TransX:" + ImgPosTrans.X.ToString();
                            }
                            else if (dX > _imgX)
                            {
                                ImgPosTrans.X = _imgX + Math.Abs(dX - _imgX);
                                _imgX = dX;
                                Status.Content = "ImgX:" + _imgX.ToString() + "Trans:" + ImgPosTrans.X.ToString();
                            }*/
                            ImgPosTrans.X = Convert.ToDouble(e.Element("x").Value);
                        }
                        if (e.Attribute("y") != null)
                        {
                            /*
                            double dY = Convert.ToDouble(e.Attribute("y").Value);
                            if (dY < _imgY)
                            {
                                ImgPosTrans.Y = _imgY - Math.Abs(dY - _imgY);
                                _imgY = dY;
                                Status.Content = "ImgY:" + _imgX.ToString() + "ScriptY:" + e.Attribute("y").Value;
                            }
                            else if (dY > _imgY)
                            {
                                ImgPosTrans.Y = _imgY + Math.Abs(dY - _imgY);
                                _imgY = dY;
                                Status.Content = "ImgY:" + _imgX.ToString() + "ScriptY:" + e.Attribute("y").Value;
                            }	*/
                            ImgPosTrans.Y = Convert.ToDouble(e.Attribute("y").Value);
                        }
                        else if (e.Element("y") != null)
                        {
                            /*
                            double dY = Convert.ToDouble(e.Element("y").Value);
                            if (dY < _imgY)
                            {
                                ImgPosTrans.Y = _imgY - Math.Abs(dY - _imgY);
                                _imgY = dY;
                                Status.Content = "ImgY:" + _imgY.ToString() + "ScriptY:" + e.Element("y").Value;
                            }
                            else if (dY > _imgY)
                            {
                                ImgPosTrans.Y = _imgY + Math.Abs(dY - _imgY);
                                _imgY = dY;
                                Status.Content = "ImgY:" + _imgY.ToString() + "ScriptY:" + e.Element("y").Value;
                            }*/
                            ImgPosTrans.Y = Convert.ToDouble(e.Element("y").Value);
                        }

                        if (e.Attribute("width") != null)
                        {
                            Img.Width = Convert.ToDouble(e.Attribute("width").Value);
                        }
                        else if (e.Element("width") != null)
                        {
                            Img.Width = Convert.ToDouble(e.Element("width").Value);
                        }
                        if (e.Attribute("height") != null)
                        {
                            Img.Height = Convert.ToDouble(e.Attribute("height").Value);
                        }
                        else if (e.Element("height") != null)
                        {
                            Img.Height = Convert.ToDouble(e.Element("height").Value);
                        }
                    }
                    #endregion
                }
#endregion
/*         
                
#region Visialbe
                        if (e.Attribute("visiable") != null)
                        {
                            bool v = Convert.ToBoolean(e.Attribute("visiable").Value);
                            if (v == false)
                            {
                                if (e.Name == "img")
                                {
                                    Img.Visibility = Visibility.Collapsed;
                                }
                                else if (e.Name == "say")
                                {
                                    Say.Visibility = Visibility.Collapsed;
                                }
                            }
                        
                        }
#endregion
 * */
                 
#region Sound
                    if (e.Name == "sound")
                    {
                        Bgm.Pause();
                        Sound.Source = new Uri("sound/" + e.Value, UriKind.RelativeOrAbsolute);
                        /*
                        if (Sound.AutoPlay)
                        {
                            Sound.AutoPlay = false;
                        }*/
                        Sound.Play();
                        _currSound = Sound;
                        Sound.MediaEnded += OnSoundEnded;
                    }
#endregion
#region GoToSection
                    if (e.Name == "gotoscene")
                    {
                        string dir = "script/" + e.Value + ".xml";
                        LoadScene(dir);
                        InitScene();
                    }
#endregion

#region Var
                    if (e.Name == "var-oper")
                    {
                        //object target = from v in _vars where (v.Key.Equals(e.Attribute("target"))) select v.Value;
                        //int? target = (from v in _vars where (v.Key.Equals(e.Attribute("target").Value)) select v.Value).First();
                        //int target = _vars[e.Attribute("target").Value];
                        switch (e.Attribute("oper").Value)
                        {
                            case "add":
                                _vars[e.Attribute("target").Value] += Convert.ToInt32(e.Attribute("value").Value);
                                /*
                                if (target is int)
                                {
                                    int value = (int)target;
                                    value += Convert.ToInt32(e.Attribute("value").Value);
                                    target = value;
                                    Status.Content = value;
                                }*/
                                    /*
                                else if (target is double)
                                {
                                    double value = (double)target;
                                    value += Convert.ToInt32(e.Attribute("value").Value);
                                    target = value;
                                    Status.Content = value;
                                }
                                     * */

                                    
                                break;
                            case "dec":
                                _vars[e.Attribute("target").Value] -= Convert.ToInt32(e.Attribute("value").Value);
                                /*
                               if (target is int)
                               {
                                   int value = (int)target;
                                   value -= Convert.ToInt32(e.Attribute("value").Value);
                                   target = value;
                                   Status.Content = value;
                               }
                                   
                               else if (target is double)
                               {
                                   double value = (double)target;
                                   value -= Convert.ToDouble(e.Attribute("value").Value);
                                   target = value;
                                   Status.Content = value;
                               }
                               */
                                break;
                            case "set":
                                _vars[e.Attribute("target").Value] = Convert.ToInt32(e.Attribute("value").Value);
                                /*
                            if (target is int)
                            {
                                int value = (int)target;
                                value = Convert.ToInt32(e.Attribute("value").Value);
                                target = value;
                                Status.Content = value;
                            }
                                
                            else if (target is double)
                            {
                                double value = (double)target;
                                value = Convert.ToInt32(e.Attribute("value").Value);
                                target = value;
                                Status.Content = value;
                            }
                            else if(target is string)
                            {
                                string value = Convert.ToString(target);
                                value = e.Attribute("value").Value;
                                target = value;
                                Status.Content = value;
                            }
                                 * */
                                break;
                        }
                    }
#endregion

#region If
                    if (e.Name == "if")
                    {
                        /*
                        object left = from v in _vars where (v.Key.Equals(e.Attribute("left"))) select v.Value;
                        object right = from v in _vars where (v.Key.Equals(e.Attribute("right"))) select v.Value;
                         * */
                        //int left = (from v in _vars where (v.Key.Equals(e.Attribute("left").Value)) select v.Value).First();
                        int left = _vars[e.Attribute("left").Value];
                        int right = 0;
                        //int? right = 0;
                        //var tmp = from v in _vars where (v.Key.Equals(e.Attribute("right").Value)) select v.Value;
                        bool isVar = _vars.ContainsKey(e.Attribute("right").Value)?true:false;
                        bool test = false;
#region 右值是变量的话
                        if (isVar)
                        {
                            right = _vars[e.Attribute("right").Value];
                            switch (e.Attribute("comp").Value)
                            {
                                case "gt":
                                    test = (left > right) ? true : false;
                                    /*
                                   if ((left is int) && (right is int))
                                   {
                                       test = ((int)left > (int)right) ? true : false;
                                   }
                                       
                                   else if ((left is double) && (right is double))
                                   {
                                       test = ((double)left > (double)right) ? true : false;
                                   }
                                   else
                                   {
                                       test = false;
                                   }
                                        * */
                                    break;
                                case "lt":
                                    test = (left < right) ? true : false;
                                    /*
                                    if ((left is int) && (right is int))
                                    {
                                        test = ((int)left < (int)right) ? true : false;
                                    }
                                        
                                    else if ((left is double) && (right is double))
                                    {
                                        test = ((double)left < (double)right) ? true : false;
                                    }
                                    else
                                    {
                                        test = false;
                                    }
                                         * */
                                    break;
                                case "equal":
                                    test = (left == right) ? true : false;
                                    /*
                                    if ((left is int) && (right is int))
                                    {
                                        test = ((int)left == (int)right) ? true : false;
                                    }
                                      
                                    else if ((left is double) && (right is double))
                                    {
                                        test = ((double)left == (double)right) ? true : false;
                                    }
                                    else if ((left is string) && (right is string))
                                    {
                                        test = ((left as string).Equals(right as string)) ? true : false;
                                    }
                                    else
                                    {
                                        test = false;
                                    }
                                         * */
                                    break;
                            }
                            if (test)
                            {
                                isIf = true;
                                _nextSectionId = e.Element("tosection").Value;
                            }
                        }
#endregion 
                        else
                        {
                            right = Convert.ToInt32(e.Attribute("right").Value);
                            switch (e.Attribute("comp").Value)
                            {
                                case "gt":
                                    test = (left > right) ? true : false;
                                    /*
                                    if (left is int)
                                    {
                                        test = ((int)left > Convert.ToInt32(right)) ? true : false;
                                    }
                                       
                                    else if (left is double)
                                    {
                                        test = ((double)left > Convert.ToDouble(right)) ? true : false;
                                    }
                                    else
                                    {
                                        test = false;
                                    }
                                         * */
                                    break;
                                case "lt":
                                    test = (left < right) ? true : false;
                                    /*
                                    if (left is int)
                                    {
                                        test = (left < Convert.ToInt32(right)) ? true : false;
                                    }
                                      
                                    else if (left is double)
                                    {
                                        test = ((double)left < Convert.ToDouble(right)) ? true : false;
                                    }
                                    else
                                    {
                                        test = false;
                                    }
                                         * */
                                    break;
                                case "equal":
                                    test = (left == right) ? true : false;
                                    /*
                                    if ((left is int))
                                    {
                                        test = (left == Convert.ToInt32(right)) ? true : false;
                                    }
                                    
                                    else if (left is double)
                                    {
                                        test = ((double)left == Convert.ToDouble(right)) ? true : false;
                                    }
                                    else if (left is string)
                                    {
                                        test = ((left as string).Equals(Convert.ToString(right))) ? true : false;
                                    }
                                    else
                                    {
                                        test = false;
                                    }
                                         * */
                                    break;
                            }
                            if (test)
                            {
                                isIf = true;
                                _nextSectionId = e.Element("tosection").Value;
                            }
                        }
#endregion
                        Status.Text = test.ToString();
                    }
            }
            if (!hasSelect&&!isIf)
            {
                _nextSectionId = _currSection.Attribute("next-section").Value;
            }
            if (findImg)
            {
                Img.Visibility = Visibility.Visible;
            }
            if (findSay)
            {
                Say.Visibility = Visibility.Visible;
            }
        } 
    

    }
}
