using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueUpdate
{
	public enum UpdaterBehavior
	{
		/// <summary>
		/// Will not show any messages, but will still show the Updater UI during updating.
		/// </summary>
		HIDDEN=0,
		/// <summary>
		/// Will show messages.
		/// </summary>
		SHOW_MESSAGES=1,
		/// <summary>
		/// Will show messages and run the updated application after the successful update.
		/// </summary>
		RUN_AFTER_UPDATE=2
	}
}
