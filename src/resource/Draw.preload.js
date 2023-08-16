SquidJS.drawFromMemoryMap = async function(id, color, bitshift) {
  const canvas = document.getElementById(id);
  const ctx = canvas.getContext('2d', { willReadFrequently: true, alpha: false });
  const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);

  const dst = await this.hostObj.Demosaicing(canvas.width, canvas.height, color, bitshift);
  const clamp = new Uint8ClampedArray(dst);
  // let hoge = dst[i] > 255 ? 255 : dst[i] < 0 ? 0 : src[i];   

  imageData.data.set(clamp);
  // let n = 0;
  // let data = imageData.data;
  // for (let i = 0; i < clamp.length; i+=4) {
  //   data[n++] = clamp[i];     // red
  //   data[n++] = clamp[i + 1]; // green
  //   data[n++] = clamp[i + 2]; // blue
  //   data[n++] = clamp[i + 3]; // a
  // }
  
  ctx.putImageData(imageData, 0, 0);
}

