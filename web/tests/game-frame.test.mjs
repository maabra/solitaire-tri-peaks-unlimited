// Lock the fixed cabinet dimensions so small windows scroll instead of shrinking the whole UI.
import {test} from 'node:test';
import assert from 'node:assert/strict';
import {frameSize} from '../game-frame.mjs';
test('game stays 1280 by 720 in every window, including phones',()=>{
 for(const [w,h] of [[1920,1080],[1075,1757],[1280,600],[700,432],[390,844],[320,640]])
  assert.deepEqual(frameSize(w,h),{width:1280,height:720});
});
