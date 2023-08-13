SquidJS.useDispatch = async function(action) {
  SquidJS._state = await SquidJS._store(SquidJS._state, action);
  SquidJS.useSelector(SquidJS._state);
}


SquidJS.createAsyncDispatch = function(initialState, reducer) {
  window.chrome.webview.addEventListener('message', async (e) => {
    let action = e.data;
    SquidJS._state = await SquidJS._store(SquidJS._state, action);
    SquidJS.useSelector(SquidJS._state);
  });
}


SquidJS.createStore = async function(initialState, reducer) {
  var newID1 = await this.hostObj.AddScript("store", `SquidJS._store = ${''+reducer};`);
  var newID2 = await this.hostObj.AddScript("initialState", `SquidJS._state = ${JSON.stringify(initialState)};`);
  return `${newID1} ${newID2}`
}


SquidJS.useSelector = async function(state) { }


SquidJS.callProcess = async function(obj) {
  var cb = '' + obj.callback;
  var result = await this.hostObj.CallProcessAsync(obj.com, obj.args, obj.window, `(${cb})()`);
  return true;
}