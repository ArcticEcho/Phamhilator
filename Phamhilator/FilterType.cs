namespace Phamhilator
{
	public enum FilterType
	{
		QuestionTitleBlackSpam,
		QuestionTitleBlackLQ,
		QuestionTitleBlackOff,
		QuestionTitleBlackName,

		QuestionBodyBlackSpam,
		QuestionBodyBlackLQ,
		QuestionBodyBlackOff,

		AnswerBlackSpam,
		AnswerBlackLQ,
		AnswerBlackOff,
		AnswerBlackName,

		QuestionTitleWhiteSpam = 100,
		QuestionTitleWhiteLQ,
		QuestionTitleWhiteOff,
		QuestionTitleWhiteName,

		QuestionBodyWhiteSpam,
		QuestionBodyWhiteLQ,
		QuestionBodyWhiteOff,

		AnswerWhiteSpam,
		AnswerWhiteLQ,
		AnswerWhiteOff,
		AnswerWhiteName
	}
}
