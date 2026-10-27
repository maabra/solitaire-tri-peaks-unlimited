// Prize zero starts at twelve o'clock and the remaining sectors run clockwise.
// Turn the disc in the opposite direction modulo one revolution to land beneath the fixed pointer.
export function wheelTargetAngle(index,current=0){
 if(!Number.isInteger(index)||index<0||index>7)throw Error('Invalid wheel segment.');
 return Math.ceil(current/360)*360+1800+(360-index*45)%360;
}

// Commit presentation only: the session has already chosen and saved this prize.
export function landWheel(wheel,index,angle=(360-index*45)%360){
 const rotor=wheel.querySelector('.wheel-rotor');
 rotor.style.transform=`rotate(${angle}deg)`;rotor.dataset.angle=angle;
 rotor.style.setProperty('--landed-angle',(angle%360)+'deg');
 wheel.classList.remove('is-spinning');wheel.classList.add('wheel-won');
 wheel.dataset.winningIndex=index;
 for(const label of rotor.querySelectorAll('[data-segment]'))label.classList.toggle('is-winning',Number(label.dataset.segment)===index);
}

// Rotate sectors and prize icons together while the pointer, hub and rim lamps stay fixed.
export async function spinWheelDisc(wheel,index,reduced=false){
 const rotor=wheel.querySelector('.wheel-rotor'),from=Number(rotor.dataset.angle)||0,to=wheelTargetAngle(index,from);
 wheel.classList.remove('wheel-won');wheel.classList.add('is-spinning');rotor.dataset.animating='true';
 rotor.querySelector('.is-winning')?.classList.remove('is-winning');
 let animation;
 try{
  if(!reduced){animation=rotor.animate([{transform:`rotate(${from}deg)`},{transform:`rotate(${to}deg)`}],{duration:3200,easing:'cubic-bezier(.12,.65,.08,1)',fill:'forwards'});await animation.finished;}
  landWheel(wheel,index,to);
 }finally{animation?.cancel();wheel.classList.remove('is-spinning');delete rotor.dataset.animating;}
}
