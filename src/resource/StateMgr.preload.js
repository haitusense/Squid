SquidJS.useDispatch = async function(action) {
  let old_state = SquidJS._state;
  SquidJS._state = await SquidJS._store(SquidJS._state, action);
  SquidJS.useSelector(old_state, SquidJS._state);
}


SquidJS.createAsyncDispatch = function(initialState, reducer) {
  window.chrome.webview.addEventListener('message', async (e) => {
    let action = e.data;
    await SquidJS.useDispatch(action);
  });
}


SquidJS.createStore = async function(key, initialState, reducer) {
  const id = await chrome.webview.hostObjects.Squid.GetScriptID(key);
  if(!id){
    var new_id = await this.hostObj.AddScript(key, `
      SquidJS._store = ${''+reducer};
      SquidJS._state = ${JSON.stringify(initialState)};
    `, true);
    return `registered ${new_id}`
  } else {
    return `registered`
  }

}


SquidJS.useSelector = async function(state) { }


SquidJS.callProcess = async function(obj) {
  var cb = '' + obj.callback;
  var result = await this.hostObj.CallProcessAsync(obj.com, obj.args, obj.window, `(${cb})()`);
  return true;
}