import {test} from 'node:test';
import assert from 'node:assert/strict';
import {frameSize} from '../game-frame.mjs';
test('desktop frame is centered-size 720p rather than a stretched window',()=>{
 assert.deepEqual(frameSize(1920,1080),{width:1280,height:720});
 const tall=frameSize(1075,1757);assert.equal(tall.width,1075);assert.equal(tall.width/tall.height,16/9);
 const short=frameSize(1280,600);assert.equal(short.height,600);assert.ok(Math.abs(short.width/short.height-16/9)<1e-12);
});
test('phones get an unscaled responsive layout',()=>{
 assert.deepEqual(frameSize(390,844),{width:390,height:720});
 assert.deepEqual(frameSize(320,640),{width:320,height:640});
});
