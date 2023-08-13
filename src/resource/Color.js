console.log("resource test");
Function.prototype.ex_method = function (name, func) {
  this.prototype[name] = func;
  return this;
};
String.ex_method('red',    function () { return `\u001b[31m${this}\u001b[0m`; });
String.ex_method('green',  function () { return `\u001b[32m${this}\u001b[0m`; });
String.ex_method('yellow', function () { return `\u001b[33m${this}\u001b[0m`; });
String.ex_method('blue',   function () { return `\u001b[34m${this}\u001b[0m`; });