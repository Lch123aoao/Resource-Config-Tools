   M S B A M L   `   `   `  ????D  @tablegen2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:tablegen2.layoutstablegen2.layouts  5      "    tablegen2.layouts.HeaderPage   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .elclr-namespace:tablegen2.layouts     5        ? DataContext  5	      ??  ?? RelativeSource ?? *Self+.??5
      ?  ? RowDefinitions 5   
   ?? 5      $??34 ?6   ?? 5      $??* ?6   ?? 5      $??50 ?6   .+?5   
   ?? $??0??6   
?????? $??1q?67   
???????._?5      ? .+?5      ??  ?? Orientation$ 
Horizontal=?6   $??Right=?66   $??Center=?6R   .+?5      #   tablegen2.layouts.ButtonLight  -   5          Name$ 	btnAddNew??5   %   #   tablegen2.layouts.ButtonLight   Click$ btnAddNew_Clicked??6m   $??60??68   $??24??6C   $??0,0,5,0q?6O   $??添加??6`   6m     5      -   5        $ 
btnAddCopy??5   %   $ btnAddCopy_Clicked??6?   $??60??6X   $??24??6c   $??0,0,5,0q?6o   $??复制添加??6?   {?69   ?? *
Editable+6?     5      -   5        $ btnEdit??5   %   $ btnEdit_Clicked??6?   $??60??6U   $??24??6`   $??0,0,5,0q?6l   $??编辑??6}   {?66   ?? *
Editable+6?     5      -   5        $ 	btnDelete??5   %   $ btnDelete_Clicked??6?   $??60??6W   $??24??6b   $??0,0,5,0q?6n   $??删除??6   {?68   ?? *
Editable+6?   5      ?? 5      $ 
Horizontal=?6   $
??Left=?66   $??Center=?6Q   .+?5        -   5        $ 	btnMoveUp??5   %   $ btnMoveUp_Clicked??6?   $??60??6W   $??24??6b   $??5 0 0 0q?6n   $??	上移↑??6   {?68   ?? *
Editable+6?     5      -   5        $ btnMoveDown??5   %   $ btnMoveDown_Clicked??6?   $??60??6Y   $??24??6d   $??5 0 0 0q?6p   $??	下移↓??6?   {?6:   ?? *
Editable+6?   5      5      5      ?? 5   
   $??1??6   
?????? $??1,0,1,0q?67   
???????._?5      ? .+?5       &    tablegen2.layouts.HeaderListView  -   5          Name$ lv??5    $   $y?Visible=?60   5!      5"      ? 5#   
   $??2??6   ???	#40000000.+?5$      ?? $??请填写文件名称：??6   $??Center=?6(   $
??Left=?6C   $??10,0,0,0q?6^   y?5%      ?? *IsNewFileMode+   BoolToVisibilityConverter ?? 	Converter# [  ?? 5&      -   5         ??Name$ txtFileName??5&      $??20??6+   $??10(?67   $??Center=?6E   $
??Left=?6`   $	??150??6{   $??	110,0,0,0q?6?   y?5'      ?? *IsNewFileMode+# [  "   tablegen2.layouts.ButtonDark  5(      "   tablegen2.layouts.ButtonDark	   Click$	 btnSave_Clicked??5*   <   $??确定保存??5(      $	??120??6+   $??30??67   $??0,0,10,0q?6C   $??Right=?5)      $??Center=?68   $
??Bold"?5*      $??16(?6.   6<   5+      5,      5-      