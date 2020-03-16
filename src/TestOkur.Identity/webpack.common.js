const path = require('path');
const webpack = require('webpack');
const extractTextPlugin = require('extract-text-webpack-plugin');
const extractCSS = new extractTextPlugin('app.css');
const copyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
	entry: {
		'index': './wwwroot/index.js',
		'signout-redirect': './wwwroot/js/signout-redirect.js'
	},
	output: {
		path: path.resolve(__dirname, 'wwwroot/dist'),
		filename: '[name].js',
		publicPath: 'dist/'
	},
	plugins: [
		extractCSS,
		new webpack.ProvidePlugin({
			$: 'jquery',
			jQuery: 'jquery',
			'window.jQuery': 'jquery'
		}),
		new copyWebpackPlugin([
			{
				from: './wwwroot/*.{png,ico,svg,xml,json}', to: './', transformPath(targetPath) {
					return targetPath.substring(targetPath.lastIndexOf('/') + 1);
				}
			}
		])
	],
	module: {
		rules: [
			{ test: /\.css$/, use: extractCSS.extract(['css-loader?minimize']) },
			{ test: /\.js?$/, use: { loader: 'babel-loader', options: { presets: ['@babel/preset-env'] } } },
			{
				test: /\.(woff(2)?|ttf|eot|svg)(\?v=\d+\.\d+\.\d+)?$/,
				use: [{
					loader: 'file-loader',
					options: {
						name: '[name].[ext]',
						outputPath: 'fonts/',
						publicPath: './fonts'
					}
				}]
			},
			{
				test: /\.(png|jp(e*)g|svg)$/,
				use: [{
					loader: 'url-loader',
					options: {
						limit: 8000, // Convert images < 8kb to base64 strings
						name: 'images/[hash]-[name].[ext]'
					}
				}]
			}

		]
	}
};