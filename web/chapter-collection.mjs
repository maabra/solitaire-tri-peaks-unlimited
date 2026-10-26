import {campaign} from './generated-content.mjs';
export const CHAPTER_SET_COUNT=campaign.chapterCount;
export const collectionIndex=chapter=>((chapter-1)%CHAPTER_SET_COUNT+CHAPTER_SET_COUNT)%CHAPTER_SET_COUNT;
