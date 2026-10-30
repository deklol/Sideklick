using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

static class Program {
 delegate IntPtr HookProc(int n, IntPtr w, IntPtr l);
 static HookProc callback=Mouse;
 static IntPtr hook;
 static int[] modes=new int[4];
 static bool[] held=new bool[4], captured=new bool[4];
 static bool listening;
 static int detected, selected;
 static Form form;
 static Label prompt;
 static ComboBox choice;
 static Button save;
 static EventWaitHandle reopen;
 static string settings=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"settings.txt");
 [StructLayout(LayoutKind.Sequential)] struct Data {public int x,y; public uint mouseData,flags,time; public UIntPtr extra;}
 [DllImport("user32.dll",SetLastError=true)] static extern IntPtr SetWindowsHookEx(int id,HookProc proc,IntPtr module,uint thread);
 [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr h);
 [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr h,int n,IntPtr w,IntPtr l);
 [DllImport("kernel32.dll",CharSet=CharSet.Auto)] static extern IntPtr GetModuleHandle(string name);
 [DllImport("user32.dll")] static extern void keybd_event(byte key,byte scan,uint flags,UIntPtr extra);
 [STAThread] static int Main(string[] args) {
  bool created;
  using(var mutex=new Mutex(true,"Local\\Sideklick",out created))
  using(reopen=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\Sideklick.Open")) {
   if(!created){reopen.Set();return 0;}
   Application.EnableVisualStyles();
   try {var lines=File.ReadAllLines(settings);for(int i=1;i<=3 && i<=lines.Length;i++){int v;if(int.TryParse(lines[i-1],out v)&&v>=0&&v<=2)modes[i]=v;}} catch(IOException){} catch(UnauthorizedAccessException){}
   hook=SetWindowsHookEx(14,callback,GetModuleHandle(null),0);
   if(hook==IntPtr.Zero){MessageBox.Show("Could not start mouse hook: "+Marshal.GetLastWin32Error(),"Sideklick");return 1;}
   if(args.Length>0&&args[0]=="/selftest"){UnhookWindowsHookEx(hook);return 0;}
   try {
    using(var timer=new System.Windows.Forms.Timer{Interval=150}) {
     timer.Tick+=delegate {
      if(reopen.WaitOne(0))Show();
      if(detected!=0 && form!=null && !form.IsDisposed){selected=detected;detected=0;
       if(selected<0){prompt.Text="That's your "+(selected==-1?"left":"right")+" click.\nChoose Detect again, then press a side button.";choice.Enabled=save.Enabled=false;}
       else {prompt.Text=Name(selected)+" detected.\nChoose what this button should do.";choice.SelectedIndex=modes[selected];choice.Enabled=save.Enabled=true;}
      }
     };
     timer.Start();if(args.Length==0 || args[0]!="--quiet")Show();Application.Run();
    }
   } finally {UnhookWindowsHookEx(hook);Release();}
  } return 0;
 }
 static string Name(int b){return b==3?"Wheel click / Mouse 3":"Side button "+b+" / Mouse "+(b+3);}
 static void Arm(){detected=selected=0;listening=true;prompt.Text="Press the mouse button you want to set up.\nSide buttons and wheel click are supported.";choice.Enabled=save.Enabled=false;}
 static Button Button(string text,int x,int y,int width){return new Button{Text=text,Left=x,Top=y,Width=width,Height=40,FlatStyle=FlatStyle.Flat,BackColor=Color.White,ForeColor=Color.FromArgb(34,43,57),Cursor=Cursors.Hand};}
 static void Show(){
  if(form!=null&&!form.IsDisposed){form.Show();form.Activate();return;}
  form=new Form{Text="Sideklick",ClientSize=new Size(520,435),BackColor=Color.FromArgb(247,249,252),Font=new Font("Segoe UI",10),StartPosition=FormStartPosition.CenterScreen,FormBorderStyle=FormBorderStyle.FixedSingle,MaximizeBox=false};
  form.Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);
  var title=new Label{Text="Sideklick",Left=28,Top=22,Width=450,Height=42,Font=new Font("Segoe UI",25,FontStyle.Bold),ForeColor=Color.FromArgb(24,35,51)};
  var subtitle=new Label{Text="Your side buttons. Your rules.",Left=30,Top=70,Width=450,Height=25,ForeColor=Color.DimGray};
  prompt=new Label{Left=30,Top=115,Width=460,Height=55};
  var detect=Button("Detect again",30,180,140);detect.Click+=delegate{Arm();};
  choice=new ComboBox{Left=30,Top=238,Width=460,DropDownStyle=ComboBoxStyle.DropDownList};
  choice.Items.AddRange(new object[]{"Keep normal behaviour","Disable button - keep your existing Discord PTT binding","Use F13 / F14 / F15 - rebind push to talk"});
  save=Button("Save & run quietly",30,285,270);save.BackColor=Color.FromArgb(38,103,224);save.ForeColor=Color.White;save.FlatAppearance.BorderSize=0;
  save.Click+=delegate{
   if(selected<1)return;
   var lines=new string[3];for(int i=1;i<=3;i++)lines[i-1]=(i==selected?choice.SelectedIndex:modes[i]).ToString();
   try{File.WriteAllLines(settings,lines);}catch(Exception ex){MessageBox.Show("Could not save: "+ex.Message,"Sideklick");return;}
   Release();modes[selected]=choice.SelectedIndex;listening=false;form.Close();
  };
  var exit=Button("Exit Sideklick",315,285,175);exit.Click+=delegate{Application.Exit();};
  var startup=new CheckBox{Text="Start Sideklick when I sign in",Left=30,Top=344,Width=460,Height=28};
  try{using(var key=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")){startup.Checked=key!=null && key.GetValue("Sideklick")!=null;}}catch{}
  startup.CheckedChanged+=delegate{
   try{using(var key=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")){if(startup.Checked)key.SetValue("Sideklick","\""+Application.ExecutablePath+"\" --quiet");else key.DeleteValue("Sideklick",false);}}
   catch(Exception ex){MessageBox.Show("Could not update startup: "+ex.Message,"Sideklick");}
  };
  var hint=new Label{Text="No tray icon. Open Sideklick again to change settings or exit.",Left=30,Top=390,Width=460,Height=30,Font=new Font("Segoe UI",9),ForeColor=Color.DimGray};
  form.Controls.AddRange(new Control[]{title,subtitle,prompt,detect,choice,save,exit,startup,hint});
  form.FormClosed+=delegate{listening=false;};Arm();form.Show();form.Activate();
 }
 static IntPtr Mouse(int n,IntPtr w,IntPtr l){
  if(n>=0){int msg=w.ToInt32(),b=0;bool down=msg==0x20B||msg==0x207;
   if(msg==0x20B||msg==0x20C){var d=(Data)Marshal.PtrToStructure(l,typeof(Data));int side=(int)(d.mouseData>>16);if(side==1||side==2)b=side;}
   else if(msg==0x207||msg==0x208)b=3;
   if(listening&&(msg==0x201||msg==0x204)){detected=msg==0x201?-1:-2;listening=false;}
   if(b>0){
    if(listening&&down){detected=b;listening=false;captured[b]=true;return new IntPtr(1);}
    if(captured[b]){if(!down)captured[b]=false;return new IntPtr(1);}
    if(modes[b]!=0){if(modes[b]==2){keybd_event((byte)(0x7B+b),0,down?0u:2u,UIntPtr.Zero);held[b]=down;}return new IntPtr(1);}
   }
  }return CallNextHookEx(hook,n,w,l);
 }
 static void Release(){for(int i=1;i<=3;i++)if(held[i]){keybd_event((byte)(0x7B+i),0,2,UIntPtr.Zero);held[i]=false;}}
}

