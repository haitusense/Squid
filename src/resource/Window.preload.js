/* 
  allow演算子にするとthisが使えない
  (クロージャーなのでobjectじゃなくてwindowの方をキャプチャする)
*/
SquidJS.setWindowState = function(obj) {
  this.hostObj.SetWindowState(
    obj.left,
    obj.top,
    obj.width,
    obj.height
  );
}