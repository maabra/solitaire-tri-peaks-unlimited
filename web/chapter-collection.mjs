import {campaign} from './generated-content.mjs';
export const CHAPTER_SET_COUNT=campaign.chapterCount;
// Map a chapter identifier to the bounded collection of original artwork sets.
export const collectionIndex=chapter=>((chapter-1)%CHAPTER_SET_COUNT+CHAPTER_SET_COUNT)%CHAPTER_SET_COUNT;
