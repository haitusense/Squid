SquidJS.addHIDEvent = function(elem, obj, act) {

  elem.addEventListener("mouseenter", async (event) => { await act(event, 'MouseEnter'); });
  
  elem.addEventListener("mouseleave", async (event) => { await act(event, 'MouseLeave'); }); 

  elem.addEventListener('keydown', async (event) => {
    if(event.repeat) return;
    let dst = `${event.shiftKey ? 'Shift+' : ''}${event.ctrlKey ? 'Ctrl+' : ''}${event.altKey ? 'Alt+' : ''}${event.code ?? ''}`;
    await act(event, dst);
  });

  elem.addEventListener('mousewheel', async (event) => {
    let key = `${event.shiftKey ? 'Shift+' : ''}${event.ctrlKey ? 'Ctrl+' : ''}${event.altKey ? 'Alt+' : ''}`;
    let wheel = event.wheelDelta > 0 ? "WheelUP" : "WheelDOWN";
    await act(event, `${key}${wheel}`)
  });

  elem.addEventListener('mousemove', async (event) => { await act(event, "MouseMove"); });

}

SquidJS.addDDEvent = function(elem, obj, act) {

  /*
    発火順 親 -(キャプチャーフェーズ)-> 子 -(バブリングフェーズ)-> 親
    バブリングフェーズ : addEventListener(,,false)
    キャプチャーフェーズ : addEventListener(,,true)
  */
  document.addEventListener('dragover', function(event) {
    event.preventDefault();  
  }, false );
  document.addEventListener('drop', function(event) {
    event.preventDefault();
  }, false);
  elem.addEventListener('drop', async function(event) {
    event.stopPropagation(); // イベント伝播をキャンセル
    event.preventDefault();  // イベントを無効
    await act(event, 'drop');
  }, false);

}